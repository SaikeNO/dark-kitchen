import { createApiClient } from "@dark-kitchen/api-client";
import { clientConfig } from "@dark-kitchen/config";
import { CreditCard, LogIn, Minus, Plus, ShoppingBag, User, X } from "lucide-react";
import { type FormEvent, useEffect, useMemo, useState } from "react";
import { appMetadata } from "./appMetadata";
import "./styles.css";

const apiClient = createApiClient(clientConfig.apiBaseUrl);

interface Theme {
  readonly primaryColor: string;
  readonly accentColor: string;
  readonly backgroundColor: string;
  readonly textColor: string;
}

interface BrandContext {
  readonly brandId: string;
  readonly brandName: string;
  readonly description: string | null;
  readonly logoUrl: string | null;
  readonly heroTitle: string | null;
  readonly heroSubtitle: string | null;
  readonly theme: Theme;
}

interface MenuResponse {
  readonly brand: BrandContext;
  readonly categories: readonly MenuCategory[];
}

interface MenuCategory {
  readonly id: string;
  readonly name: string;
  readonly products: readonly MenuProduct[];
}

interface MenuProduct {
  readonly id: string;
  readonly categoryId: string;
  readonly name: string;
  readonly description: string | null;
  readonly imageUrl: string | null;
  readonly price: number;
  readonly currency: string;
}

interface CartLine {
  readonly menuItemId: string;
  readonly quantity: number;
}

interface CartResponse {
  readonly cartId: string;
  readonly brandId: string;
  readonly totalPrice: number;
  readonly currency: string;
  readonly items: readonly CartItemResponse[];
}

interface CartItemResponse {
  readonly menuItemId: string;
  readonly name: string;
  readonly imageUrl: string | null;
  readonly quantity: number;
  readonly unitPrice: number;
  readonly currency: string;
  readonly lineTotal: number;
}

interface Session {
  readonly id: string;
  readonly email: string;
  readonly displayName: string | null;
  readonly phone: string | null;
}

interface CheckoutResponse {
  readonly paymentId: string;
  readonly paymentStatus: string;
  readonly orderId: string | null;
  readonly correlationId: string | null;
  readonly failureReason: string | null;
}

interface CustomerForm {
  readonly displayName: string;
  readonly phone: string;
  readonly deliveryNote: string;
}

export function App() {
  const [menu, setMenu] = useState<MenuResponse | null>(null);
  const [cart, setCart] = useState<CartResponse | null>(null);
  const [cartOpen, setCartOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [session, setSession] = useState<Session | null>(null);
  const [authOpen, setAuthOpen] = useState(false);
  const [paymentMode, setPaymentMode] = useState<"success" | "failed">("success");
  const [customer, setCustomer] = useState<CustomerForm>({ displayName: "", phone: "", deliveryNote: "" });
  const [checkout, setCheckout] = useState<CheckoutResponse | null>(null);
  const [busy, setBusy] = useState(false);

  const brandQuery = useMemo(() => brandQueryString(), []);
  const productsById = useMemo(() => {
    const products = menu?.categories.flatMap(category => category.products) ?? [];
    return new Map(products.map(product => [product.id, product]));
  }, [menu]);

  useEffect(() => {
    let active = true;
    async function load() {
      try {
        const [loadedMenu, loadedSession] = await Promise.all([
          getJson<MenuResponse>(`/api/storefront/menu${brandQuery}`),
          getJson<Session | null>("/api/storefront/auth/me")
        ]);
        if (!active) {
          return;
        }

        setMenu(loadedMenu);
        setSession(loadedSession);
        applyTheme(loadedMenu.brand.theme);
        const stored = readStoredCart(loadedMenu.brand.brandId);
        const created = await postJson<CartResponse>(`/api/storefront/carts${brandQuery}`, { cartId: stored?.cartId ?? null });
        const synced = stored?.items.length
          ? await patchCart(created.cartId, stored.items, brandQuery)
          : created;
        if (active) {
          setCart(synced);
          writeStoredCart(loadedMenu.brand.brandId, synced);
        }
      } catch (loadError) {
        if (active) {
          setError(loadError instanceof Error ? loadError.message : "Nie udało się załadować sklepu.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void load();
    return () => {
      active = false;
    };
  }, [brandQuery]);

  async function setQuantity(product: MenuProduct, quantity: number) {
    if (cart === null || menu === null) {
      return;
    }

    const nextItems = cart.items
      .filter(item => item.menuItemId !== product.id)
      .map(item => ({ menuItemId: item.menuItemId, quantity: item.quantity }));
    if (quantity > 0) {
      nextItems.push({ menuItemId: product.id, quantity });
    }

    const synced = await patchCart(cart.cartId, nextItems, brandQuery);
    setCart(synced);
    writeStoredCart(menu.brand.brandId, synced);
  }

  async function submitCheckout(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (cart === null || cart.items.length === 0) {
      return;
    }

    setBusy(true);
    setCheckout(null);
    try {
      const response = await postJson<CheckoutResponse>(`/api/storefront/checkout${brandQuery}`, {
        cartId: cart.cartId,
        customer,
        mockPaymentResult: paymentMode
      });
      setCheckout(response);
      if (response.paymentStatus === "Success" && menu !== null) {
        const emptied = await patchCart(cart.cartId, [], brandQuery);
        setCart(emptied);
        writeStoredCart(menu.brand.brandId, emptied);
      }
    } catch (checkoutError) {
      setError(checkoutError instanceof Error ? checkoutError.message : "Checkout failed.");
    } finally {
      setBusy(false);
    }
  }

  if (loading) {
    return <main className="storefront-screen center"><p>Ładowanie sklepu...</p></main>;
  }

  if (error !== null || menu === null) {
    return (
      <main className="storefront-screen center">
        <h1>{appMetadata.name}</h1>
        <p>{error ?? "Sklep niedostępny."}</p>
      </main>
    );
  }

  const totalItems = cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0;

  return (
    <main className="storefront-shell">
      <header className="topbar">
        <div className="brand-lockup">
          {menu.brand.logoUrl !== null && <img src={menu.brand.logoUrl} alt="" />}
          <div>
            <p>Dark Kitchen</p>
            <h1>{menu.brand.brandName}</h1>
          </div>
        </div>
        <div className="topbar-actions">
          <button className="ghost-button" type="button" onClick={() => setAuthOpen(true)}>
            <User aria-hidden="true" />
            {session?.displayName ?? session?.email ?? "Konto"}
          </button>
          <button className="cart-button" type="button" onClick={() => setCartOpen(true)}>
            <ShoppingBag aria-hidden="true" />
            Koszyk
            <strong>{totalItems}</strong>
          </button>
        </div>
      </header>

      <section className="hero">
        <div>
          <p className="eyebrow">White-label Storefront</p>
          <h2>{menu.brand.heroTitle ?? menu.brand.brandName}</h2>
          <p>{menu.brand.heroSubtitle ?? menu.brand.description ?? "Wybierz pozycje, opłać mock payment i wyślij zamówienie do kuchni."}</p>
        </div>
      </section>

      <section className="menu-layout">
        <div className="menu-list">
          {menu.categories.map(category => (
            <section key={category.id} className="category-section">
              <h3>{category.name}</h3>
              <div className="product-grid">
                {category.products.map(product => {
                  const quantity = cart?.items.find(item => item.menuItemId === product.id)?.quantity ?? 0;
                  return (
                    <article key={product.id} className="product-card">
                      <div className="product-image">
                        {product.imageUrl === null ? <span>{product.name}</span> : <img src={product.imageUrl} alt="" />}
                      </div>
                      <div className="product-body">
                        <h4>{product.name}</h4>
                        <p>{product.description ?? "Pozycja menu gotowa do checkoutu."}</p>
                        <div className="product-footer">
                          <strong>{money(product.price, product.currency)}</strong>
                          {quantity === 0 ? (
                            <button type="button" onClick={() => { void setQuantity(product, 1); }}>
                              <Plus aria-hidden="true" /> Dodaj
                            </button>
                          ) : (
                            <div className="stepper">
                              <button type="button" aria-label={`Usuń ${product.name}`} onClick={() => { void setQuantity(product, quantity - 1); }}><Minus aria-hidden="true" /></button>
                              <span>{quantity}</span>
                              <button type="button" aria-label={`Dodaj ${product.name}`} onClick={() => { void setQuantity(product, quantity + 1); }}><Plus aria-hidden="true" /></button>
                            </div>
                          )}
                        </div>
                      </div>
                    </article>
                  );
                })}
              </div>
            </section>
          ))}
        </div>

        <aside className="checkout-panel">
          <h3>Checkout</h3>
          <CartSummary cart={cart} productsById={productsById} onQuantity={setQuantity} />
          <form onSubmit={event => { void submitCheckout(event); }}>
            <label>Imię
              <input value={customer.displayName} onChange={event => setCustomer({ ...customer, displayName: event.currentTarget.value })} />
            </label>
            <label>Telefon
              <input value={customer.phone} onChange={event => setCustomer({ ...customer, phone: event.currentTarget.value })} />
            </label>
            <label>Notatka
              <textarea value={customer.deliveryNote} onChange={event => setCustomer({ ...customer, deliveryNote: event.currentTarget.value })} />
            </label>
            <label>Mock payment
              <select value={paymentMode} onChange={event => setPaymentMode(event.currentTarget.value as "success" | "failed")}>
                <option value="success">Success</option>
                <option value="failed">Failed</option>
              </select>
            </label>
            <button className="checkout-button" type="submit" disabled={busy || cart === null || cart.items.length === 0}>
              <CreditCard aria-hidden="true" />
              Zapłać i zamów
            </button>
          </form>
          {checkout !== null && (
            <div className={checkout.paymentStatus === "Success" ? "result success" : "result failed"}>
              <strong>{checkout.paymentStatus}</strong>
              <span>{checkout.orderId === null ? checkout.failureReason : `Order ${checkout.orderId}`}</span>
            </div>
          )}
        </aside>
      </section>

      {cartOpen && (
        <div className="drawer-backdrop">
          <button className="drawer-scrim" type="button" aria-label="Zamknij koszyk" onClick={() => setCartOpen(false)} />
          <aside className="cart-drawer">
            <button className="icon-button" type="button" aria-label="Zamknij koszyk" onClick={() => setCartOpen(false)}><X aria-hidden="true" /></button>
            <h3>Koszyk</h3>
            <CartSummary cart={cart} productsById={productsById} onQuantity={setQuantity} />
          </aside>
        </div>
      )}

      {authOpen && <AuthDialog session={session} onSession={setSession} onClose={() => setAuthOpen(false)} />}
    </main>
  );
}

function CartSummary({
  cart,
  productsById,
  onQuantity
}: {
  readonly cart: CartResponse | null;
  readonly productsById: ReadonlyMap<string, MenuProduct>;
  readonly onQuantity: (product: MenuProduct, quantity: number) => Promise<void>;
}) {
  if (cart === null || cart.items.length === 0) {
    return <p className="empty-cart">Koszyk pusty.</p>;
  }

  return (
    <div className="cart-summary">
      {cart.items.map(item => {
        const product = productsById.get(item.menuItemId);
        return (
          <div key={item.menuItemId} className="cart-line">
            <div>
              <strong>{item.name}</strong>
              <span>{item.quantity} x {money(item.unitPrice, item.currency)}</span>
            </div>
            {product !== undefined && (
              <div className="stepper small">
                <button type="button" onClick={() => { void onQuantity(product, item.quantity - 1); }}><Minus aria-hidden="true" /></button>
                <span>{item.quantity}</span>
                <button type="button" onClick={() => { void onQuantity(product, item.quantity + 1); }}><Plus aria-hidden="true" /></button>
              </div>
            )}
          </div>
        );
      })}
      <div className="cart-total">
        <span>Razem</span>
        <strong>{money(cart.totalPrice, cart.currency)}</strong>
      </div>
    </div>
  );
}

function AuthDialog({
  session,
  onSession,
  onClose
}: {
  readonly session: Session | null;
  readonly onSession: (session: Session | null) => void;
  readonly onClose: () => void;
}) {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [phone, setPhone] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    try {
      const response = await postJson<Session>(mode === "login" ? "/api/storefront/auth/login" : "/api/storefront/auth/register", {
        email,
        password,
        displayName,
        phone
      });
      onSession(response);
      onClose();
    } catch (authError) {
      setError(authError instanceof Error ? authError.message : "Logowanie nieudane.");
    }
  }

  async function logout() {
    await postJson<void>("/api/storefront/auth/logout", {});
    onSession(null);
    onClose();
  }

  return (
    <div className="drawer-backdrop">
      <section className="auth-dialog">
        <button className="icon-button" type="button" aria-label="Zamknij konto" onClick={onClose}><X aria-hidden="true" /></button>
        <h3>Konto klienta</h3>
        {session === null ? (
          <form onSubmit={event => { void submit(event); }}>
            <div className="segmented">
              <button type="button" className={mode === "login" ? "active" : ""} onClick={() => setMode("login")}>Login</button>
              <button type="button" className={mode === "register" ? "active" : ""} onClick={() => setMode("register")}>Register</button>
            </div>
            <label>Email<input type="email" value={email} onChange={event => setEmail(event.currentTarget.value)} /></label>
            <label>Password<input type="password" value={password} onChange={event => setPassword(event.currentTarget.value)} /></label>
            {mode === "register" && (
              <>
                <label>Imię<input value={displayName} onChange={event => setDisplayName(event.currentTarget.value)} /></label>
                <label>Telefon<input value={phone} onChange={event => setPhone(event.currentTarget.value)} /></label>
              </>
            )}
            <button className="checkout-button" type="submit"><LogIn aria-hidden="true" /> Zapisz sesję</button>
            {error !== null && <p className="auth-error">{error}</p>}
          </form>
        ) : (
          <div className="account-box">
            <strong>{session.displayName ?? session.email}</strong>
            <span>{session.email}</span>
            <button type="button" onClick={() => { void logout(); }}>Logout</button>
          </div>
        )}
      </section>
    </div>
  );
}

function applyTheme(theme: Theme) {
  const root = document.documentElement;
  root.style.setProperty("--sf-primary", theme.primaryColor);
  root.style.setProperty("--sf-accent", theme.accentColor);
  root.style.setProperty("--sf-background", theme.backgroundColor);
  root.style.setProperty("--sf-text", theme.textColor);
}

async function getJson<T>(path: string) {
  const response = await fetch(apiClient.buildUrl(path), { credentials: "include" });
  return await readResponse<T>(response);
}

async function postJson<T>(path: string, body: unknown) {
  const response = await fetch(apiClient.buildUrl(path), {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body)
  });
  return await readResponse<T>(response);
}

async function patchCart(cartId: string, items: readonly CartLine[], brandQuery: string) {
  const response = await fetch(apiClient.buildUrl(`/api/storefront/carts/${cartId}${brandQuery}`), {
    method: "PATCH",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ items })
  });
  return await readResponse<CartResponse>(response);
}

async function readResponse<T>(response: Response) {
  if (!response.ok) {
    throw new Error(`API ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return await response.json() as T;
}

function money(amount: number, currency: string) {
  return new Intl.NumberFormat("pl-PL", { style: "currency", currency }).format(amount);
}

function brandQueryString() {
  const brandId = new URLSearchParams(window.location.search).get("brandId");
  return brandId === null ? "" : `?brandId=${encodeURIComponent(brandId)}`;
}

function storageKey(brandId: string) {
  return `dark-kitchen-storefront-cart-${brandId}`;
}

function readStoredCart(brandId: string): { readonly cartId: string; readonly items: readonly CartLine[] } | null {
  const raw = localStorage.getItem(storageKey(brandId));
  if (raw === null) {
    return null;
  }

  try {
    return JSON.parse(raw) as { readonly cartId: string; readonly items: readonly CartLine[] };
  } catch {
    return null;
  }
}

function writeStoredCart(brandId: string, cart: CartResponse) {
  localStorage.setItem(storageKey(brandId), JSON.stringify({
    cartId: cart.cartId,
    items: cart.items.map(item => ({ menuItemId: item.menuItemId, quantity: item.quantity }))
  }));
}
