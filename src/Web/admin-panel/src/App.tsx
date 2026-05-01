import { createApiClient } from "@dark-kitchen/api-client";
import { clientConfig } from "@dark-kitchen/config";
import { type FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import { appMetadata } from "./appMetadata";
import "./styles.css";

const apiClient = createApiClient(clientConfig.apiBaseUrl);
const demoPassword = "Demo123!";

type AuthStatus = "checking" | "signed-out" | "signed-in";
type TabId = "brands" | "menu" | "recipes" | "stations";

interface Session {
  readonly email: string;
  readonly roles: string[];
}

interface Brand {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly logoUrl: string | null;
  readonly isActive: boolean;
}

interface Category {
  readonly id: string;
  readonly brandId: string;
  readonly name: string;
  readonly sortOrder: number;
  readonly isActive: boolean;
}

interface Product {
  readonly id: string;
  readonly brandId: string;
  readonly categoryId: string;
  readonly name: string;
  readonly description: string | null;
  readonly price: number;
  readonly currency: string;
  readonly isActive: boolean;
  readonly stationId: string | null;
  readonly stationCode: string | null;
  readonly recipeItemCount: number;
}

interface Ingredient {
  readonly id: string;
  readonly name: string;
  readonly unit: string;
  readonly isActive: boolean;
}

interface Station {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
  readonly isActive: boolean;
}

interface Recipe {
  readonly productId: string;
  readonly items: RecipeItem[];
}

interface RecipeItem {
  readonly ingredientId: string;
  readonly ingredientName: string;
  readonly unit: string;
  readonly quantity: number;
}

interface ApiProblem {
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Record<string, string[]>;
}

interface BrandForm {
  readonly name: string;
  readonly description: string;
  readonly logoUrl: string;
  readonly isActive: boolean;
}

interface CategoryForm {
  readonly brandId: string;
  readonly name: string;
  readonly sortOrder: string;
  readonly isActive: boolean;
}

interface ProductForm {
  readonly brandId: string;
  readonly categoryId: string;
  readonly name: string;
  readonly description: string;
  readonly price: string;
  readonly currency: string;
}

interface IngredientForm {
  readonly name: string;
  readonly unit: string;
  readonly isActive: boolean;
}

interface StationForm {
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
  readonly isActive: boolean;
}

interface RecipeDraftRow {
  readonly ingredientId: string;
  readonly quantity: string;
}

const emptyBrandForm: BrandForm = {
  name: "",
  description: "",
  logoUrl: "",
  isActive: true
};

const emptyCategoryForm: CategoryForm = {
  brandId: "",
  name: "",
  sortOrder: "10",
  isActive: true
};

const emptyProductForm: ProductForm = {
  brandId: "",
  categoryId: "",
  name: "",
  description: "",
  price: "0.00",
  currency: "PLN"
};

const emptyIngredientForm: IngredientForm = {
  name: "",
  unit: "g",
  isActive: true
};

const emptyStationForm: StationForm = {
  code: "",
  name: "",
  displayColor: "#2f7d57",
  isActive: true
};

const tabs = [
  { id: "brands", label: "Brands" },
  { id: "menu", label: "Menu" },
  { id: "recipes", label: "Recipes" },
  { id: "stations", label: "Stations" }
] as const satisfies readonly { readonly id: TabId; readonly label: string }[];

function problemMessage(problem: ApiProblem | undefined, fallback: string) {
  if (problem?.errors !== undefined) {
    return Object.entries(problem.errors)
      .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
      .join(" ");
  }

  return problem?.detail ?? problem?.title ?? fallback;
}

async function requestApi<TResponse>(
  path: string,
  init: RequestInit & { readonly body?: BodyInit | null } = {}
) {
  const headers = new Headers(init.headers);
  const hasBody = init.body !== undefined && init.body !== null;
  if (hasBody && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(apiClient.buildUrl(path), {
    ...init,
    headers,
    credentials: "include"
  });

  if (!response.ok) {
    const problem = (await response.json().catch(() => undefined)) as ApiProblem | undefined;
    throw new Error(problemMessage(problem, `Request failed with ${response.status}.`));
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

async function loadCatalogSnapshot() {
  const [brands, categories, products, ingredients, stations] = await Promise.all([
    requestApi<Brand[]>("/api/admin/brands"),
    requestApi<Category[]>("/api/admin/categories"),
    requestApi<Product[]>("/api/admin/products"),
    requestApi<Ingredient[]>("/api/admin/ingredients"),
    requestApi<Station[]>("/api/admin/stations")
  ]);

  return { brands, categories, products, ingredients, stations };
}

function textOrDash(value: string | null | undefined) {
  return value === null || value === undefined || value.length === 0 ? "-" : value;
}

function statusLabel(isActive: boolean) {
  return isActive ? "Active" : "Inactive";
}

export function App() {
  const [authStatus, setAuthStatus] = useState<AuthStatus>("checking");
  const [session, setSession] = useState<Session | null>(null);
  const [activeTab, setActiveTab] = useState<TabId>("brands");
  const [email, setEmail] = useState("manager@darkkitchen.local");
  const [password, setPassword] = useState(demoPassword);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const [brands, setBrands] = useState<Brand[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [ingredients, setIngredients] = useState<Ingredient[]>([]);
  const [stations, setStations] = useState<Station[]>([]);

  const [brandForm, setBrandForm] = useState<BrandForm>(emptyBrandForm);
  const [editingBrandId, setEditingBrandId] = useState<string | null>(null);
  const [categoryForm, setCategoryForm] = useState<CategoryForm>(emptyCategoryForm);
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
  const [productForm, setProductForm] = useState<ProductForm>(emptyProductForm);
  const [editingProductId, setEditingProductId] = useState<string | null>(null);
  const [ingredientForm, setIngredientForm] = useState<IngredientForm>(emptyIngredientForm);
  const [editingIngredientId, setEditingIngredientId] = useState<string | null>(null);
  const [stationForm, setStationForm] = useState<StationForm>(emptyStationForm);
  const [editingStationId, setEditingStationId] = useState<string | null>(null);
  const [selectedRecipeProductId, setSelectedRecipeProductId] = useState("");
  const [recipeRows, setRecipeRows] = useState<RecipeDraftRow[]>([]);
  const [routeDrafts, setRouteDrafts] = useState<Record<string, string>>({});

  const isManager = session?.roles.includes("Manager") ?? false;
  const canWrite = authStatus === "signed-in" && isManager;

  const brandById = useMemo(() => new Map(brands.map(brand => [brand.id, brand])), [brands]);
  const categoryById = useMemo(() => new Map(categories.map(category => [category.id, category])), [categories]);

  const activeBrands = brands.filter(brand => brand.isActive);
  const activeCategories = categories.filter(category => category.isActive);
  const activeStations = stations.filter(station => station.isActive);
  const activeIngredients = ingredients.filter(ingredient => ingredient.isActive);
  const effectiveRecipeProductId = selectedRecipeProductId.length > 0
    ? selectedRecipeProductId
    : products[0]?.id ?? "";
  const productCategories = productForm.brandId.length > 0
    ? activeCategories.filter(category => category.brandId === productForm.brandId)
    : activeCategories;

  const refreshCatalog = useCallback(async () => {
    const snapshot = await loadCatalogSnapshot();

    setBrands(snapshot.brands);
    setCategories(snapshot.categories);
    setProducts(snapshot.products);
    setIngredients(snapshot.ingredients);
    setStations(snapshot.stations);
  }, []);

  useEffect(() => {
    let mounted = true;

    async function bootstrap() {
      try {
        const current = await requestApi<Session>("/api/admin/auth/me");
        if (!mounted) {
          return;
        }

        setSession(current);
        setAuthStatus("signed-in");
      } catch {
        if (mounted) {
          setSession(null);
          setAuthStatus("signed-out");
        }
      }
    }

    void bootstrap();

    return () => {
      mounted = false;
    };
  }, []);

  useEffect(() => {
    if (authStatus !== "signed-in") {
      return;
    }

    let mounted = true;

    async function loadInitialCatalog() {
      try {
        const snapshot = await loadCatalogSnapshot();
        if (!mounted) {
          return;
        }

        setBrands(snapshot.brands);
        setCategories(snapshot.categories);
        setProducts(snapshot.products);
        setIngredients(snapshot.ingredients);
        setStations(snapshot.stations);
      } catch (loadError) {
        if (mounted) {
          setError(loadError instanceof Error ? loadError.message : "Catalog load failed.");
        }
      }
    }

    void loadInitialCatalog();

    return () => {
      mounted = false;
    };
  }, [authStatus]);

  useEffect(() => {
    if (authStatus !== "signed-in" || effectiveRecipeProductId.length === 0) {
      return;
    }

    let mounted = true;

    async function loadRecipe() {
      try {
        const recipe = await requestApi<Recipe>(`/api/admin/products/${effectiveRecipeProductId}/recipe`);
        if (mounted) {
          setRecipeRows(recipe.items.map(item => ({
            ingredientId: item.ingredientId,
            quantity: String(item.quantity)
          })));
        }
      } catch (loadError) {
        if (mounted) {
          setError(loadError instanceof Error ? loadError.message : "Recipe load failed.");
        }
      }
    }

    void loadRecipe();

    return () => {
      mounted = false;
    };
  }, [authStatus, effectiveRecipeProductId]);

  function clearMessages() {
    setError(null);
    setNotice(null);
  }

  function requireManager() {
    if (!canWrite) {
      setError("Operator can only read catalog data.");
      return false;
    }

    return true;
  }

  async function runMutation(action: () => Promise<void>, success: string) {
    if (!requireManager()) {
      return;
    }

    clearMessages();
    setBusy(true);
    try {
      await action();
      await refreshCatalog();
      setNotice(success);
    } catch (mutationError) {
      setError(mutationError instanceof Error ? mutationError.message : "Request failed.");
    } finally {
      setBusy(false);
    }
  }

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearMessages();
    setBusy(true);

    try {
      const current = await requestApi<Session>("/api/admin/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password })
      });
      setSession(current);
      setAuthStatus("signed-in");
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : "Login failed.");
    } finally {
      setBusy(false);
    }
  }

  async function handleLogout() {
    clearMessages();
    setBusy(true);
    try {
      await requestApi<void>("/api/admin/auth/logout", { method: "POST" });
    } finally {
      setSession(null);
      setAuthStatus("signed-out");
      setBusy(false);
    }
  }

  function pickDemoAccount(nextEmail: string) {
    setEmail(nextEmail);
    setPassword(demoPassword);
  }

  function editBrand(brand: Brand) {
    setEditingBrandId(brand.id);
    setBrandForm({
      name: brand.name,
      description: brand.description ?? "",
      logoUrl: brand.logoUrl ?? "",
      isActive: brand.isActive
    });
  }

  async function saveBrand(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runMutation(async () => {
      const payload = {
        name: brandForm.name,
        description: brandForm.description,
        logoUrl: brandForm.logoUrl,
        isActive: brandForm.isActive
      };

      if (editingBrandId === null) {
        await requestApi<Brand>("/api/admin/brands", { method: "POST", body: JSON.stringify(payload) });
      } else {
        await requestApi<Brand>(`/api/admin/brands/${editingBrandId}`, { method: "PUT", body: JSON.stringify(payload) });
      }

      setEditingBrandId(null);
      setBrandForm(emptyBrandForm);
    }, "Brand saved.");
  }

  function editCategory(category: Category) {
    setEditingCategoryId(category.id);
    setCategoryForm({
      brandId: category.brandId,
      name: category.name,
      sortOrder: String(category.sortOrder),
      isActive: category.isActive
    });
  }

  async function saveCategory(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runMutation(async () => {
      const payload = {
        brandId: categoryForm.brandId,
        name: categoryForm.name,
        sortOrder: Number(categoryForm.sortOrder),
        isActive: categoryForm.isActive
      };

      if (editingCategoryId === null) {
        await requestApi<Category>("/api/admin/categories", { method: "POST", body: JSON.stringify(payload) });
      } else {
        await requestApi<Category>(`/api/admin/categories/${editingCategoryId}`, {
          method: "PUT",
          body: JSON.stringify(payload)
        });
      }

      setEditingCategoryId(null);
      setCategoryForm(emptyCategoryForm);
    }, "Category saved.");
  }

  function editProduct(product: Product) {
    setEditingProductId(product.id);
    setProductForm({
      brandId: product.brandId,
      categoryId: product.categoryId,
      name: product.name,
      description: product.description ?? "",
      price: String(product.price),
      currency: product.currency
    });
  }

  async function saveProduct(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runMutation(async () => {
      const payload = {
        brandId: productForm.brandId,
        categoryId: productForm.categoryId,
        name: productForm.name,
        description: productForm.description,
        price: Number(productForm.price),
        currency: productForm.currency
      };

      if (editingProductId === null) {
        await requestApi<Product>("/api/admin/products", { method: "POST", body: JSON.stringify(payload) });
      } else {
        await requestApi<Product>(`/api/admin/products/${editingProductId}`, {
          method: "PUT",
          body: JSON.stringify(payload)
        });
      }

      setEditingProductId(null);
      setProductForm(emptyProductForm);
    }, "Product saved.");
  }

  function editIngredient(ingredient: Ingredient) {
    setEditingIngredientId(ingredient.id);
    setIngredientForm({
      name: ingredient.name,
      unit: ingredient.unit,
      isActive: ingredient.isActive
    });
  }

  async function saveIngredient(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runMutation(async () => {
      const payload = {
        name: ingredientForm.name,
        unit: ingredientForm.unit,
        isActive: ingredientForm.isActive
      };

      if (editingIngredientId === null) {
        await requestApi<Ingredient>("/api/admin/ingredients", { method: "POST", body: JSON.stringify(payload) });
      } else {
        await requestApi<Ingredient>(`/api/admin/ingredients/${editingIngredientId}`, {
          method: "PUT",
          body: JSON.stringify(payload)
        });
      }

      setEditingIngredientId(null);
      setIngredientForm(emptyIngredientForm);
    }, "Ingredient saved.");
  }

  function editStation(station: Station) {
    setEditingStationId(station.id);
    setStationForm({
      code: station.code,
      name: station.name,
      displayColor: station.displayColor,
      isActive: station.isActive
    });
  }

  async function saveStation(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runMutation(async () => {
      const payload = {
        code: stationForm.code,
        name: stationForm.name,
        displayColor: stationForm.displayColor,
        isActive: stationForm.isActive
      };

      if (editingStationId === null) {
        await requestApi<Station>("/api/admin/stations", { method: "POST", body: JSON.stringify(payload) });
      } else {
        await requestApi<Station>(`/api/admin/stations/${editingStationId}`, {
          method: "PUT",
          body: JSON.stringify(payload)
        });
      }

      setEditingStationId(null);
      setStationForm(emptyStationForm);
    }, "Station saved.");
  }

  async function deactivate(path: string, success: string) {
    await runMutation(async () => {
      await requestApi<unknown>(path, { method: "POST" });
    }, success);
  }

  async function activateProduct(productId: string) {
    await runMutation(async () => {
      await requestApi<Product>(`/api/admin/products/${productId}/activate`, { method: "POST" });
    }, "Product activated.");
  }

  async function saveRoute(productId: string) {
    await runMutation(async () => {
      const stationId = routeDrafts[productId] ?? "";
      await requestApi<unknown>(`/api/admin/products/${productId}/station-route`, {
        method: "PUT",
        body: JSON.stringify({ stationId })
      });
    }, "Route saved.");
  }

  async function saveRecipe(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runMutation(async () => {
      await requestApi<Recipe>(`/api/admin/products/${effectiveRecipeProductId}/recipe`, {
        method: "PUT",
        body: JSON.stringify({
          items: recipeRows.map(row => ({
            ingredientId: row.ingredientId,
            quantity: Number(row.quantity)
          }))
        })
      });
    }, "Recipe saved.");
  }

  function addRecipeRow() {
    setRecipeRows(current => [
      ...current,
      { ingredientId: activeIngredients[0]?.id ?? "", quantity: "1" }
    ]);
  }

  if (authStatus === "checking") {
    return (
      <main className="admin-shell">
        <header className="topbar">
          <div>
            <p className="brand-mark">Dark Kitchen</p>
            <h1>{appMetadata.name}</h1>
          </div>
          <span className="pill">Loading</span>
        </header>
      </main>
    );
  }

  if (authStatus === "signed-out") {
    return (
      <main className="login-screen">
        <section className="login-panel" aria-labelledby="login-title">
          <p className="brand-mark">Dark Kitchen</p>
          <h1 id="login-title">{appMetadata.name}</h1>
          <form className="login-form" onSubmit={event => void handleLogin(event)}>
            <label>
              Email
              <input
                autoComplete="username"
                type="email"
                value={email}
                onChange={event => setEmail(event.currentTarget.value)}
              />
            </label>
            <label>
              Password
              <input
                autoComplete="current-password"
                type="password"
                value={password}
                onChange={event => setPassword(event.currentTarget.value)}
              />
            </label>
            <div className="demo-row" aria-label="Demo accounts">
              <button type="button" onClick={() => pickDemoAccount("manager@darkkitchen.local")}>
                Manager demo
              </button>
              <button type="button" onClick={() => pickDemoAccount("operator@darkkitchen.local")}>
                Operator demo
              </button>
            </div>
            {error !== null && <p className="message is-error">{error}</p>}
            <button className="primary-button" type="submit" disabled={busy}>
              Sign in
            </button>
          </form>
        </section>
      </main>
    );
  }

  return (
    <main className="admin-shell">
      <header className="topbar">
        <div>
          <p className="brand-mark">Dark Kitchen</p>
          <h1>{appMetadata.name}</h1>
        </div>
        <div className="session-box">
          <span>{session?.email}</span>
          <span className={isManager ? "pill is-manager" : "pill"}>{isManager ? "Manager" : "Operator"}</span>
          <button type="button" onClick={() => void handleLogout()} disabled={busy}>
            Logout
          </button>
        </div>
      </header>

      <nav className="tabs" aria-label="Admin sections">
        {tabs.map(tab => (
          <button
            key={tab.id}
            type="button"
            className={tab.id === activeTab ? "is-selected" : ""}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </nav>

      {(error !== null || notice !== null || !canWrite) && (
        <div className="message-row">
          {!canWrite && <p className="message">Operator read-only mode</p>}
          {notice !== null && <p className="message is-success">{notice}</p>}
          {error !== null && <p className="message is-error">{error}</p>}
        </div>
      )}

      {activeTab === "brands" && renderBrands()}
      {activeTab === "menu" && renderMenu()}
      {activeTab === "recipes" && renderRecipes()}
      {activeTab === "stations" && renderStations()}
    </main>
  );

  function renderBrands() {
    return (
      <section className="workspace" aria-labelledby="brands-title">
        <div className="section-heading">
          <h2 id="brands-title">Brands</h2>
          <span>{brands.length} records</span>
        </div>
        <div className="split-layout">
          <form className="editor-panel" onSubmit={event => void saveBrand(event)}>
            <h3>{editingBrandId === null ? "New brand" : "Edit brand"}</h3>
            <label>
              Name
              <input
                value={brandForm.name}
                disabled={!canWrite}
                onChange={event => setBrandForm({ ...brandForm, name: event.currentTarget.value })}
              />
            </label>
            <label>
              Description
              <textarea
                value={brandForm.description}
                disabled={!canWrite}
                onChange={event => setBrandForm({ ...brandForm, description: event.currentTarget.value })}
              />
            </label>
            <label>
              Logo URL
              <input
                value={brandForm.logoUrl}
                disabled={!canWrite}
                onChange={event => setBrandForm({ ...brandForm, logoUrl: event.currentTarget.value })}
              />
            </label>
            <label className="checkbox-line">
              <input
                type="checkbox"
                checked={brandForm.isActive}
                disabled={!canWrite}
                onChange={event => setBrandForm({ ...brandForm, isActive: event.currentTarget.checked })}
              />
              Active
            </label>
            <div className="form-actions">
              <button className="primary-button" type="submit" disabled={!canWrite || busy}>
                Save
              </button>
              <button
                type="button"
                onClick={() => {
                  setEditingBrandId(null);
                  setBrandForm(emptyBrandForm);
                }}
              >
                Clear
              </button>
            </div>
          </form>
          <div className="table-panel">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Status</th>
                  <th>Description</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {brands.map(brand => (
                  <tr key={brand.id}>
                    <td>{brand.name}</td>
                    <td><span className={brand.isActive ? "status is-active" : "status"}>{statusLabel(brand.isActive)}</span></td>
                    <td>{textOrDash(brand.description)}</td>
                    <td className="row-actions">
                      <button type="button" onClick={() => editBrand(brand)}>Edit</button>
                      <button
                        type="button"
                        disabled={!canWrite || !brand.isActive || busy}
                        onClick={() => void deactivate(`/api/admin/brands/${brand.id}/deactivate`, "Brand deactivated.")}
                      >
                        Deactivate
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    );
  }

  function renderMenu() {
    return (
      <section className="workspace" aria-labelledby="menu-title">
        <div className="section-heading">
          <h2 id="menu-title">Menu</h2>
          <span>{products.length} products</span>
        </div>
        <div className="two-column">
          <form className="editor-panel" onSubmit={event => void saveCategory(event)}>
            <h3>{editingCategoryId === null ? "New category" : "Edit category"}</h3>
            <label>
              Brand
              <select
                value={categoryForm.brandId}
                disabled={!canWrite}
                onChange={event => setCategoryForm({ ...categoryForm, brandId: event.currentTarget.value })}
              >
                <option value="">Select brand</option>
                {activeBrands.map(brand => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
              </select>
            </label>
            <label>
              Name
              <input
                value={categoryForm.name}
                disabled={!canWrite}
                onChange={event => setCategoryForm({ ...categoryForm, name: event.currentTarget.value })}
              />
            </label>
            <label>
              Sort order
              <input
                type="number"
                value={categoryForm.sortOrder}
                disabled={!canWrite}
                onChange={event => setCategoryForm({ ...categoryForm, sortOrder: event.currentTarget.value })}
              />
            </label>
            <label className="checkbox-line">
              <input
                type="checkbox"
                checked={categoryForm.isActive}
                disabled={!canWrite}
                onChange={event => setCategoryForm({ ...categoryForm, isActive: event.currentTarget.checked })}
              />
              Active
            </label>
            <div className="form-actions">
              <button className="primary-button" type="submit" disabled={!canWrite || busy}>Save</button>
              <button
                type="button"
                onClick={() => {
                  setEditingCategoryId(null);
                  setCategoryForm(emptyCategoryForm);
                }}
              >
                Clear
              </button>
            </div>
          </form>

          <form className="editor-panel" onSubmit={event => void saveProduct(event)}>
            <h3>{editingProductId === null ? "New product" : "Edit product"}</h3>
            <label>
              Brand
              <select
                value={productForm.brandId}
                disabled={!canWrite}
                onChange={event => setProductForm({ ...productForm, brandId: event.currentTarget.value, categoryId: "" })}
              >
                <option value="">Select brand</option>
                {activeBrands.map(brand => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
              </select>
            </label>
            <label>
              Category
              <select
                value={productForm.categoryId}
                disabled={!canWrite}
                onChange={event => setProductForm({ ...productForm, categoryId: event.currentTarget.value })}
              >
                <option value="">Select category</option>
                {productCategories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
              </select>
            </label>
            <label>
              Name
              <input
                value={productForm.name}
                disabled={!canWrite}
                onChange={event => setProductForm({ ...productForm, name: event.currentTarget.value })}
              />
            </label>
            <label>
              Description
              <textarea
                value={productForm.description}
                disabled={!canWrite}
                onChange={event => setProductForm({ ...productForm, description: event.currentTarget.value })}
              />
            </label>
            <div className="inline-fields">
              <label>
                Price
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={productForm.price}
                  disabled={!canWrite}
                  onChange={event => setProductForm({ ...productForm, price: event.currentTarget.value })}
                />
              </label>
              <label>
                Currency
                <input
                  value={productForm.currency}
                  disabled={!canWrite}
                  onChange={event => setProductForm({ ...productForm, currency: event.currentTarget.value })}
                />
              </label>
            </div>
            <div className="form-actions">
              <button className="primary-button" type="submit" disabled={!canWrite || busy}>Save</button>
              <button
                type="button"
                onClick={() => {
                  setEditingProductId(null);
                  setProductForm(emptyProductForm);
                }}
              >
                Clear
              </button>
            </div>
          </form>
        </div>

        <div className="table-panel">
          <table>
            <thead>
              <tr>
                <th>Product</th>
                <th>Brand</th>
                <th>Category</th>
                <th>Price</th>
                <th>Status</th>
                <th>Recipe</th>
                <th>Station</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map(product => (
                <tr key={product.id}>
                  <td>{product.name}</td>
                  <td>{brandById.get(product.brandId)?.name ?? "-"}</td>
                  <td>{categoryById.get(product.categoryId)?.name ?? "-"}</td>
                  <td>{product.price.toFixed(2)} {product.currency}</td>
                  <td><span className={product.isActive ? "status is-active" : "status"}>{statusLabel(product.isActive)}</span></td>
                  <td>{product.recipeItemCount}</td>
                  <td>
                    <select
                      className="compact-select"
                      value={routeDrafts[product.id] ?? product.stationId ?? ""}
                      disabled={!canWrite}
                      onChange={event => setRouteDrafts({ ...routeDrafts, [product.id]: event.currentTarget.value })}
                      aria-label={`Station for ${product.name}`}
                    >
                      <option value="">No station</option>
                      {activeStations.map(station => <option key={station.id} value={station.id}>{station.code}</option>)}
                    </select>
                  </td>
                  <td className="row-actions">
                    <button type="button" onClick={() => editProduct(product)}>Edit</button>
                    <button type="button" disabled={!canWrite || busy} onClick={() => void saveRoute(product.id)}>Route</button>
                    <button type="button" disabled={!canWrite || product.isActive || busy} onClick={() => void activateProduct(product.id)}>Activate</button>
                    <button
                      type="button"
                      disabled={!canWrite || !product.isActive || busy}
                      onClick={() => void deactivate(`/api/admin/products/${product.id}/deactivate`, "Product deactivated.")}
                    >
                      Deactivate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="table-panel">
          <table>
            <thead>
              <tr>
                <th>Category</th>
                <th>Brand</th>
                <th>Order</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {categories.map(category => (
                <tr key={category.id}>
                  <td>{category.name}</td>
                  <td>{brandById.get(category.brandId)?.name ?? "-"}</td>
                  <td>{category.sortOrder}</td>
                  <td><span className={category.isActive ? "status is-active" : "status"}>{statusLabel(category.isActive)}</span></td>
                  <td className="row-actions">
                    <button type="button" onClick={() => editCategory(category)}>Edit</button>
                    <button
                      type="button"
                      disabled={!canWrite || !category.isActive || busy}
                      onClick={() => void deactivate(`/api/admin/categories/${category.id}/deactivate`, "Category deactivated.")}
                    >
                      Deactivate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    );
  }

  function renderRecipes() {
    const selectedProduct = products.find(product => product.id === effectiveRecipeProductId);

    return (
      <section className="workspace" aria-labelledby="recipes-title">
        <div className="section-heading">
          <h2 id="recipes-title">Recipes</h2>
          <span>{ingredients.length} ingredients</span>
        </div>
        <div className="two-column">
          <form className="editor-panel" onSubmit={event => void saveIngredient(event)}>
            <h3>{editingIngredientId === null ? "New ingredient" : "Edit ingredient"}</h3>
            <label>
              Name
              <input
                value={ingredientForm.name}
                disabled={!canWrite}
                onChange={event => setIngredientForm({ ...ingredientForm, name: event.currentTarget.value })}
              />
            </label>
            <label>
              Unit
              <input
                value={ingredientForm.unit}
                disabled={!canWrite}
                onChange={event => setIngredientForm({ ...ingredientForm, unit: event.currentTarget.value })}
              />
            </label>
            <label className="checkbox-line">
              <input
                type="checkbox"
                checked={ingredientForm.isActive}
                disabled={!canWrite}
                onChange={event => setIngredientForm({ ...ingredientForm, isActive: event.currentTarget.checked })}
              />
              Active
            </label>
            <div className="form-actions">
              <button className="primary-button" type="submit" disabled={!canWrite || busy}>Save</button>
              <button
                type="button"
                onClick={() => {
                  setEditingIngredientId(null);
                  setIngredientForm(emptyIngredientForm);
                }}
              >
                Clear
              </button>
            </div>
          </form>

          <form className="editor-panel recipe-editor" onSubmit={event => void saveRecipe(event)}>
            <h3>{selectedProduct?.name ?? "Recipe"}</h3>
            <label>
              Product
              <select
                value={effectiveRecipeProductId}
                onChange={event => setSelectedRecipeProductId(event.currentTarget.value)}
              >
                <option value="">Select product</option>
                {products.map(product => <option key={product.id} value={product.id}>{product.name}</option>)}
              </select>
            </label>
            <div className="recipe-rows">
              {recipeRows.map((row, index) => (
                <div className="recipe-row" key={`${row.ingredientId}-${index.toString()}`}>
                  <select
                    value={row.ingredientId}
                    disabled={!canWrite}
                    aria-label={`Ingredient ${index + 1}`}
                    onChange={event => setRecipeRows(current => current.map((item, itemIndex) => (
                      itemIndex === index ? { ...item, ingredientId: event.currentTarget.value } : item
                    )))}
                  >
                    <option value="">Ingredient</option>
                    {activeIngredients.map(ingredient => (
                      <option key={ingredient.id} value={ingredient.id}>{ingredient.name} ({ingredient.unit})</option>
                    ))}
                  </select>
                  <input
                    aria-label={`Quantity ${index + 1}`}
                    type="number"
                    min="0"
                    step="0.001"
                    value={row.quantity}
                    disabled={!canWrite}
                    onChange={event => setRecipeRows(current => current.map((item, itemIndex) => (
                      itemIndex === index ? { ...item, quantity: event.currentTarget.value } : item
                    )))}
                  />
                  <button
                    type="button"
                    disabled={!canWrite}
                    onClick={() => setRecipeRows(current => current.filter((_row, itemIndex) => itemIndex !== index))}
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
            <div className="form-actions">
              <button type="button" disabled={!canWrite || activeIngredients.length === 0} onClick={addRecipeRow}>
                Add item
              </button>
              <button className="primary-button" type="submit" disabled={!canWrite || busy || effectiveRecipeProductId.length === 0}>
                Save recipe
              </button>
            </div>
          </form>
        </div>

        <div className="table-panel">
          <table>
            <thead>
              <tr>
                <th>Ingredient</th>
                <th>Unit</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {ingredients.map(ingredient => (
                <tr key={ingredient.id}>
                  <td>{ingredient.name}</td>
                  <td>{ingredient.unit}</td>
                  <td><span className={ingredient.isActive ? "status is-active" : "status"}>{statusLabel(ingredient.isActive)}</span></td>
                  <td className="row-actions">
                    <button type="button" onClick={() => editIngredient(ingredient)}>Edit</button>
                    <button
                      type="button"
                      disabled={!canWrite || !ingredient.isActive || busy}
                      onClick={() => void deactivate(`/api/admin/ingredients/${ingredient.id}/deactivate`, "Ingredient deactivated.")}
                    >
                      Deactivate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    );
  }

  function renderStations() {
    return (
      <section className="workspace" aria-labelledby="stations-title">
        <div className="section-heading">
          <h2 id="stations-title">Stations</h2>
          <span>{stations.length} records</span>
        </div>
        <div className="split-layout">
          <form className="editor-panel" onSubmit={event => void saveStation(event)}>
            <h3>{editingStationId === null ? "New station" : "Edit station"}</h3>
            <label>
              Code
              <input
                value={stationForm.code}
                disabled={!canWrite}
                onChange={event => setStationForm({ ...stationForm, code: event.currentTarget.value })}
              />
            </label>
            <label>
              Name
              <input
                value={stationForm.name}
                disabled={!canWrite}
                onChange={event => setStationForm({ ...stationForm, name: event.currentTarget.value })}
              />
            </label>
            <label>
              Color
              <input
                type="color"
                value={stationForm.displayColor}
                disabled={!canWrite}
                onChange={event => setStationForm({ ...stationForm, displayColor: event.currentTarget.value })}
              />
            </label>
            <label className="checkbox-line">
              <input
                type="checkbox"
                checked={stationForm.isActive}
                disabled={!canWrite}
                onChange={event => setStationForm({ ...stationForm, isActive: event.currentTarget.checked })}
              />
              Active
            </label>
            <div className="form-actions">
              <button className="primary-button" type="submit" disabled={!canWrite || busy}>Save</button>
              <button
                type="button"
                onClick={() => {
                  setEditingStationId(null);
                  setStationForm(emptyStationForm);
                }}
              >
                Clear
              </button>
            </div>
          </form>
          <div className="table-panel">
            <table>
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Color</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {stations.map(station => (
                  <tr key={station.id}>
                    <td>{station.code}</td>
                    <td>{station.name}</td>
                    <td>
                      <span className="color-chip" style={{ backgroundColor: station.displayColor }} />
                      {station.displayColor}
                    </td>
                    <td><span className={station.isActive ? "status is-active" : "status"}>{statusLabel(station.isActive)}</span></td>
                    <td className="row-actions">
                      <button type="button" onClick={() => editStation(station)}>Edit</button>
                      <button
                        type="button"
                        disabled={!canWrite || !station.isActive || busy}
                        onClick={() => void deactivate(`/api/admin/stations/${station.id}/deactivate`, "Station deactivated.")}
                      >
                        Deactivate
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    );
  }
}
