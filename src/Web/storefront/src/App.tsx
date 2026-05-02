import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ArrowRight,
  CheckCircle2,
  ChefHat,
  CreditCard,
  LogIn,
  Minus,
  Plus,
  Search,
  ShoppingBag,
  Store,
  SunMoon,
  User,
  X
} from "lucide-react";
import { type CSSProperties, type FormEvent, type ReactNode, useEffect, useMemo, useState } from "react";
import { Link, Navigate, Route, Routes, useParams, useSearchParams } from "react-router-dom";
import {
  type BrandContext,
  type CartLine,
  type CartResponse,
  type CustomerForm,
  type MenuProduct,
  type Session,
  type Theme,
  checkout,
  createCart,
  getMenu,
  getSession,
  listBrands,
  login,
  logout,
  register,
  updateCart
} from "./api/storefrontApi";
import { errorMessage } from "./api/http";
import { appMetadata } from "./appMetadata";
import "./styles.css";

const emptyCustomer: CustomerForm = { displayName: "", phone: "", deliveryNote: "" };
type ColorMode = "light" | "dark";

export function App() {
  const [colorMode, setColorMode] = useState<ColorMode>(() => readColorMode());

  useEffect(() => {
    document.documentElement.dataset.theme = colorMode;
    localStorage.setItem("dark-kitchen-storefront-theme", colorMode);
  }, [colorMode]);

  function toggleColorMode() {
    setColorMode(current => current === "light" ? "dark" : "light");
  }

  return (
    <Routes>
      <Route path="/" element={<BrandPicker colorMode={colorMode} onToggleColorMode={toggleColorMode} />} />
      <Route path="/brands/:brandId" element={<StorefrontPage colorMode={colorMode} onToggleColorMode={toggleColorMode} />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function BrandPicker({
  colorMode,
  onToggleColorMode
}: {
  readonly colorMode: ColorMode;
  readonly onToggleColorMode: () => void;
}) {
  const [searchParams] = useSearchParams();
  const legacyBrandId = searchParams.get("brandId");
  const brandsQuery = useQuery({
    queryKey: ["storefront", "brands"],
    queryFn: ({ signal }) => listBrands(signal)
  });

  if (legacyBrandId !== null && legacyBrandId.length > 0) {
    return <Navigate to={`/brands/${encodeURIComponent(legacyBrandId)}`} replace />;
  }

  return (
    <main className="brand-select-shell">
      <header className="brand-select-header">
        <div className="brand-select-title">
          <div className="store-mark"><Store aria-hidden="true" /></div>
          <div>
            <p>Dark Kitchen</p>
            <h1>Wybierz marke</h1>
          </div>
        </div>
        <ThemeToggle colorMode={colorMode} onToggle={onToggleColorMode} />
      </header>

      <section className="brand-select-hero">
        <p className="eyebrow">Storefront</p>
        <h2>Zamow z aktywnej marki</h2>
        <p>Menu, kolory i logo pochodza z konfiguracji w panelu admina.</p>
      </section>

      {brandsQuery.isPending && <StateMessage label="Ladowanie marek..." />}
      {brandsQuery.isError && <StateMessage label={errorMessage(brandsQuery.error, "Nie udalo sie pobrac marek.")} />}

      {brandsQuery.data !== undefined && (
        <div className="brand-grid">
          {brandsQuery.data.map(brand => (
            <Link
              key={brand.brandId}
              className="brand-card"
              style={themeStyle(brand.theme)}
              to={`/brands/${brand.brandId}`}
            >
              <BrandAvatar brand={brand} />
              <div>
                <h3>{brand.brandName}</h3>
                <p>{brand.heroSubtitle ?? brand.description ?? "Menu gotowe do checkoutu."}</p>
              </div>
              <ArrowRight aria-hidden="true" />
            </Link>
          ))}
        </div>
      )}
    </main>
  );
}

function StorefrontPage({
  colorMode,
  onToggleColorMode
}: {
  readonly colorMode: ColorMode;
  readonly onToggleColorMode: () => void;
}) {
  const routeBrandId = useParams().brandId;
  const brandId = routeBrandId ?? "";
  const queryClient = useQueryClient();
  const [cartOpen, setCartOpen] = useState(false);
  const [authOpen, setAuthOpen] = useState(false);
  const [paymentMode, setPaymentMode] = useState<"success" | "failed">("success");
  const [customer, setCustomer] = useState<CustomerForm>(emptyCustomer);
  const [checkoutResult, setCheckoutResult] = useState<null | { readonly status: string; readonly message: string }>(null);

  const menuQuery = useQuery({
    queryKey: ["storefront", "menu", brandId],
    queryFn: ({ signal }) => getMenu(brandId, signal),
    enabled: brandId.length > 0
  });
  const sessionQuery = useQuery({
    queryKey: ["storefront", "session"],
    queryFn: ({ signal }) => getSession(signal)
  });
  const cartQuery = useQuery({
    queryKey: cartKey(brandId),
    queryFn: () => prepareCart(brandId),
    enabled: brandId.length > 0
  });

  const menu = menuQuery.data;
  const cart = cartQuery.data;
  const session = sessionQuery.data ?? null;
  const productsById = useMemo(() => {
    const products = menu?.categories.flatMap(category => category.products) ?? [];
    return new Map(products.map(product => [product.id, product]));
  }, [menu]);

  useEffect(() => {
    if (menu !== undefined) {
      applyTheme(menu.brand.theme);
    }
  }, [menu]);

  const cartMutation = useMutation({
    mutationFn: (items: readonly CartLine[]) => {
      const current = queryClient.getQueryData<CartResponse>(cartKey(brandId));
      if (current === undefined) {
        throw new Error("Koszyk nie jest gotowy.");
      }

      return updateCart(brandId, current.cartId, items);
    },
    onSuccess: updated => {
      queryClient.setQueryData(cartKey(brandId), updated);
      writeStoredCart(brandId, updated);
    }
  });

  const checkoutMutation = useMutation({
    mutationFn: () => {
      if (cart === undefined) {
        throw new Error("Koszyk nie jest gotowy.");
      }

      return checkout(brandId, cart.cartId, customer, paymentMode);
    }
  });

  if (routeBrandId === undefined) {
    return <Navigate to="/" replace />;
  }

  function setQuantity(product: MenuProduct, quantity: number) {
    if (cart === undefined) {
      return;
    }

    const nextItems = cart.items
      .filter(item => item.menuItemId !== product.id)
      .map(item => ({ menuItemId: item.menuItemId, quantity: item.quantity }));
    if (quantity > 0) {
      nextItems.push({ menuItemId: product.id, quantity });
    }

    cartMutation.mutate(nextItems);
  }

  async function submitCheckout(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (cart === undefined || cart.items.length === 0) {
      return;
    }

    setCheckoutResult(null);
    try {
      const response = await checkoutMutation.mutateAsync();
      if (response.paymentStatus === "Success") {
        await cartMutation.mutateAsync([]);
        setCheckoutResult({ status: "success", message: `Order ${response.orderId ?? ""}`.trim() });
        return;
      }

      setCheckoutResult({ status: "failed", message: response.failureReason ?? "Platnosc odrzucona." });
    } catch (error) {
      setCheckoutResult({ status: "failed", message: errorMessage(error, "Checkout nie powiodl sie.") });
    }
  }

  if (menuQuery.isPending || cartQuery.isPending) {
    return <StateMessage label="Ladowanie sklepu..." />;
  }

  if (menuQuery.isError || cartQuery.isError || menu === undefined || cart === undefined) {
    return <StateMessage label={errorMessage(menuQuery.error ?? cartQuery.error, "Sklep niedostepny.")} />;
  }

  const totalItems = cart.items.reduce((sum, item) => sum + item.quantity, 0);
  const busy = checkoutMutation.isPending || cartMutation.isPending;

  return (
    <main className="storefront-shell">
      <header className="topbar">
        <Link className="brand-lockup" to="/">
          <BrandAvatar brand={menu.brand} />
          <div>
            <p>Dark Kitchen</p>
            <h1>{menu.brand.brandName}</h1>
          </div>
        </Link>
        <div className="topbar-actions">
          <ThemeToggle colorMode={colorMode} onToggle={onToggleColorMode} />
          <button className="ghost-button" type="button" onClick={() => setAuthOpen(true)}>
            <User aria-hidden="true" />
            <span>{session?.displayName ?? session?.email ?? "Konto"}</span>
          </button>
          <button className="cart-button desktop-cart-button" type="button" onClick={() => setCartOpen(true)}>
            <ShoppingBag aria-hidden="true" />
            <span>Koszyk</span>
            <strong>{totalItems}</strong>
          </button>
        </div>
      </header>

      <section className="hero">
        <div>
          <p className="eyebrow">White-label Storefront</p>
          <h2>{menu.brand.heroTitle ?? menu.brand.brandName}</h2>
          <p>{menu.brand.heroSubtitle ?? menu.brand.description ?? "Szybki checkout, swieze menu, status zamowienia w OMS."}</p>
        </div>
      </section>

      <nav className="category-tabs" aria-label="Kategorie menu">
        {menu.categories.map(category => (
          <a key={category.id} href={`#category-${category.id}`}>{category.name}</a>
        ))}
      </nav>

      <section className="menu-layout">
        <div className="menu-list">
          {menu.categories.map(category => (
            <section key={category.id} id={`category-${category.id}`} className="category-section">
              <div className="category-heading">
                <h3>{category.name}</h3>
                <span>{category.products.length} pozycji</span>
              </div>
              <div className="product-grid">
                {category.products.map(product => (
                  <ProductCard
                    key={product.id}
                    product={product}
                    quantity={cart.items.find(item => item.menuItemId === product.id)?.quantity ?? 0}
                    onQuantity={setQuantity}
                  />
                ))}
              </div>
            </section>
          ))}
        </div>

        <aside className="checkout-panel">
          <CheckoutBox
            cart={cart}
            productsById={productsById}
            customer={customer}
            paymentMode={paymentMode}
            checkoutResult={checkoutResult}
            busy={busy}
            onCustomer={setCustomer}
            onPaymentMode={setPaymentMode}
            onQuantity={setQuantity}
            onSubmit={submitCheckout}
          />
        </aside>
      </section>

      <button className="mobile-cart-bar" type="button" onClick={() => setCartOpen(true)}>
        <ShoppingBag aria-hidden="true" />
        <span>{totalItems === 0 ? "Koszyk pusty" : `${totalItems} w koszyku`}</span>
        <strong>{money(cart.totalPrice, cart.currency)}</strong>
      </button>

      {cartOpen && (
        <Drawer title="Koszyk" onClose={() => setCartOpen(false)}>
          <CheckoutBox
            cart={cart}
            productsById={productsById}
            customer={customer}
            paymentMode={paymentMode}
            checkoutResult={checkoutResult}
            busy={busy}
            onCustomer={setCustomer}
            onPaymentMode={setPaymentMode}
            onQuantity={setQuantity}
            onSubmit={submitCheckout}
          />
        </Drawer>
      )}

      {authOpen && (
        <AuthDialog
          session={session}
          onSession={value => queryClient.setQueryData(["storefront", "session"], value)}
          onClose={() => setAuthOpen(false)}
        />
      )}
    </main>
  );
}

function ProductCard({
  product,
  quantity,
  onQuantity
}: {
  readonly product: MenuProduct;
  readonly quantity: number;
  readonly onQuantity: (product: MenuProduct, quantity: number) => void;
}) {
  return (
    <article className="product-card">
      <div className="product-image">
        {product.imageUrl === null
          ? <ChefHat aria-hidden="true" />
          : <img src={product.imageUrl} alt={product.name} />}
      </div>
      <div className="product-body">
        <div>
          <h4>{product.name}</h4>
          <p>{product.description ?? "Gotowe do zamowienia."}</p>
        </div>
        <div className="product-footer">
          <strong>{money(product.price, product.currency)}</strong>
          {quantity === 0 ? (
            <button type="button" onClick={() => onQuantity(product, 1)}>
              <Plus aria-hidden="true" /> Dodaj
            </button>
          ) : (
            <QuantityStepper
              label={product.name}
              quantity={quantity}
              onMinus={() => onQuantity(product, quantity - 1)}
              onPlus={() => onQuantity(product, quantity + 1)}
            />
          )}
        </div>
      </div>
    </article>
  );
}

function CheckoutBox({
  cart,
  productsById,
  customer,
  paymentMode,
  checkoutResult,
  busy,
  onCustomer,
  onPaymentMode,
  onQuantity,
  onSubmit
}: {
  readonly cart: CartResponse;
  readonly productsById: ReadonlyMap<string, MenuProduct>;
  readonly customer: CustomerForm;
  readonly paymentMode: "success" | "failed";
  readonly checkoutResult: null | { readonly status: string; readonly message: string };
  readonly busy: boolean;
  readonly onCustomer: (customer: CustomerForm) => void;
  readonly onPaymentMode: (mode: "success" | "failed") => void;
  readonly onQuantity: (product: MenuProduct, quantity: number) => void;
  readonly onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
}) {
  return (
    <div className="checkout-box">
      <div className="panel-title">
        <h3>Checkout</h3>
        <span>{cart.items.length} pozycji</span>
      </div>
      <CartSummary cart={cart} productsById={productsById} onQuantity={onQuantity} />
      <form onSubmit={event => { void onSubmit(event); }}>
        <label>Imie
          <input value={customer.displayName} onChange={event => onCustomer({ ...customer, displayName: event.currentTarget.value })} />
        </label>
        <label>Telefon
          <input inputMode="tel" value={customer.phone} onChange={event => onCustomer({ ...customer, phone: event.currentTarget.value })} />
        </label>
        <label>Notatka
          <textarea value={customer.deliveryNote} onChange={event => onCustomer({ ...customer, deliveryNote: event.currentTarget.value })} />
        </label>
        <label>Mock payment
          <select value={paymentMode} onChange={event => onPaymentMode(event.currentTarget.value as "success" | "failed")}>
            <option value="success">Success</option>
            <option value="failed">Failed</option>
          </select>
        </label>
        <button className="checkout-button" type="submit" disabled={busy || cart.items.length === 0}>
          <CreditCard aria-hidden="true" />
          Zamow za {money(cart.totalPrice, cart.currency)}
        </button>
      </form>
      {checkoutResult !== null && (
        <div className={checkoutResult.status === "success" ? "result success" : "result failed"}>
          {checkoutResult.status === "success" && <CheckCircle2 aria-hidden="true" />}
          <span>{checkoutResult.message}</span>
        </div>
      )}
    </div>
  );
}

function CartSummary({
  cart,
  productsById,
  onQuantity
}: {
  readonly cart: CartResponse;
  readonly productsById: ReadonlyMap<string, MenuProduct>;
  readonly onQuantity: (product: MenuProduct, quantity: number) => void;
}) {
  if (cart.items.length === 0) {
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
              <QuantityStepper
                label={item.name}
                quantity={item.quantity}
                small
                onMinus={() => onQuantity(product, item.quantity - 1)}
                onPlus={() => onQuantity(product, item.quantity + 1)}
              />
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

function QuantityStepper({
  label,
  quantity,
  small = false,
  onMinus,
  onPlus
}: {
  readonly label: string;
  readonly quantity: number;
  readonly small?: boolean;
  readonly onMinus: () => void;
  readonly onPlus: () => void;
}) {
  return (
    <div className={small ? "stepper small" : "stepper"}>
      <button type="button" aria-label={`Usun ${label}`} onClick={onMinus}><Minus aria-hidden="true" /></button>
      <span>{quantity}</span>
      <button type="button" aria-label={`Dodaj ${label}`} onClick={onPlus}><Plus aria-hidden="true" /></button>
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

  const authMutation = useMutation({
    mutationFn: () => mode === "login"
      ? login(email, password)
      : register(email, password, displayName, phone),
    onSuccess: value => {
      onSession(value);
      onClose();
    },
    onError: value => setError(errorMessage(value, "Logowanie nieudane."))
  });
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSuccess: () => {
      onSession(null);
      onClose();
    }
  });

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    authMutation.mutate();
  }

  return (
    <Drawer title="Konto klienta" onClose={onClose} compact>
      {session === null ? (
        <form className="auth-form" onSubmit={submit}>
          <div className="segmented">
            <button type="button" className={mode === "login" ? "active" : ""} onClick={() => setMode("login")}>Login</button>
            <button type="button" className={mode === "register" ? "active" : ""} onClick={() => setMode("register")}>Register</button>
          </div>
          <label>Email<input type="email" value={email} onChange={event => setEmail(event.currentTarget.value)} /></label>
          <label>Password<input type="password" value={password} onChange={event => setPassword(event.currentTarget.value)} /></label>
          {mode === "register" && (
            <>
              <label>Imie<input value={displayName} onChange={event => setDisplayName(event.currentTarget.value)} /></label>
              <label>Telefon<input inputMode="tel" value={phone} onChange={event => setPhone(event.currentTarget.value)} /></label>
            </>
          )}
          <button className="checkout-button" type="submit" disabled={authMutation.isPending}>
            <LogIn aria-hidden="true" /> Zapisz sesje
          </button>
          {error !== null && <p className="auth-error">{error}</p>}
        </form>
      ) : (
        <div className="account-box">
          <strong>{session.displayName ?? session.email}</strong>
          <span>{session.email}</span>
          <button type="button" onClick={() => logoutMutation.mutate()} disabled={logoutMutation.isPending}>Logout</button>
        </div>
      )}
    </Drawer>
  );
}

function Drawer({
  title,
  compact = false,
  onClose,
  children
}: {
  readonly title: string;
  readonly compact?: boolean;
  readonly onClose: () => void;
  readonly children: ReactNode;
}) {
  return (
    <div className="drawer-backdrop">
      <button className="drawer-scrim" type="button" aria-label={`Zamknij ${title}`} onClick={onClose} />
      <aside className={compact ? "cart-drawer auth-drawer" : "cart-drawer"} role="dialog" aria-modal="true" aria-label={title}>
        <button className="icon-button" type="button" aria-label={`Zamknij ${title}`} onClick={onClose}><X aria-hidden="true" /></button>
        {children}
      </aside>
    </div>
  );
}

function BrandAvatar({ brand }: { readonly brand: BrandContext }) {
  if (brand.logoUrl !== null) {
    return <img className="brand-avatar" src={brand.logoUrl} alt={brand.brandName} />;
  }

  return <span className="brand-avatar fallback"><ChefHat aria-hidden="true" /></span>;
}

function ThemeToggle({
  colorMode,
  onToggle
}: {
  readonly colorMode: ColorMode;
  readonly onToggle: () => void;
}) {
  return (
    <button className="theme-toggle" type="button" onClick={onToggle} aria-label="Zmien motyw">
      <SunMoon aria-hidden="true" />
      <span>{colorMode === "light" ? "Light" : "Dark"}</span>
    </button>
  );
}

function StateMessage({ label }: { readonly label: string }) {
  return (
    <main className="state-screen">
      <Search aria-hidden="true" />
      <h1>{appMetadata.name}</h1>
      <p>{label}</p>
    </main>
  );
}

function readColorMode(): ColorMode {
  return localStorage.getItem("dark-kitchen-storefront-theme") === "dark" ? "dark" : "light";
}

async function prepareCart(brandId: string) {
  const stored = readStoredCart(brandId);
  const created = await createCart(brandId, stored?.cartId ?? null);
  if (stored === null || stored.items.length === 0) {
    writeStoredCart(brandId, created);
    return created;
  }

  try {
    const synced = await updateCart(brandId, created.cartId, stored.items);
    writeStoredCart(brandId, synced);
    return synced;
  } catch {
    localStorage.removeItem(storageKey(brandId));
    writeStoredCart(brandId, created);
    return created;
  }
}

function applyTheme(theme: Theme) {
  const root = document.documentElement;
  root.style.setProperty("--sf-primary", theme.primaryColor);
  root.style.setProperty("--sf-accent", theme.accentColor);
  root.style.setProperty("--sf-background", theme.backgroundColor);
  root.style.setProperty("--sf-text", theme.textColor);
}

function themeStyle(theme: Theme) {
  return {
    "--sf-primary": theme.primaryColor,
    "--sf-accent": theme.accentColor,
    "--sf-background": theme.backgroundColor,
    "--sf-text": theme.textColor
  } as CSSProperties;
}

function cartKey(brandId: string) {
  return ["storefront", "cart", brandId] as const;
}

function money(amount: number, currency: string) {
  return new Intl.NumberFormat("pl-PL", { style: "currency", currency }).format(amount);
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
