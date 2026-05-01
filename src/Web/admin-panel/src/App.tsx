import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  BookOpen,
  Boxes,
  ChefHat,
  CircleAlert,
  CircleCheck,
  ClipboardList,
  Database,
  KeyRound,
  LogOut,
  Package,
  Pencil,
  Plus,
  Power,
  RefreshCw,
  Route,
  Save,
  ShieldCheck,
  Utensils,
  UserRound,
  X,
  type LucideIcon
} from "lucide-react";
import { type FormEvent, type ReactNode, useMemo, useState } from "react";
import {
  activateProduct as activateProductRequest,
  apiConfigured,
  deactivateBrand,
  deactivateCategory,
  deactivateIngredient,
  deactivateProduct,
  deactivateStation,
  getCatalogSnapshot,
  getCurrentSession,
  getRecipe,
  loginAdmin,
  logoutAdmin,
  saveBrand as saveBrandRequest,
  saveCategory as saveCategoryRequest,
  saveIngredient as saveIngredientRequest,
  saveProduct as saveProductRequest,
  saveProductStationRoute,
  saveRecipe as saveRecipeRequest,
  saveStation as saveStationRequest,
  type Brand,
  type CatalogSnapshot,
  type Category,
  type Ingredient,
  type Product,
  type Session,
  type Station
} from "./adminApi";
import { appMetadata } from "./appMetadata";
import "./styles.css";

const demoPassword = "Demo123!";

type TabId = "brands" | "menu" | "recipes" | "stations";

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

interface RecipeDraft {
  readonly productId: string;
  readonly rows: RecipeDraftRow[];
}

interface CatalogMutationRequest {
  readonly action: () => Promise<unknown>;
  readonly recipeProductId?: string;
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
  displayColor: "#22c55e",
  isActive: true
};

const emptyCatalogSnapshot: CatalogSnapshot = {
  brands: [],
  categories: [],
  products: [],
  ingredients: [],
  stations: []
};

const queryKeys = {
  session: ["admin-session"] as const,
  catalog: ["admin-catalog"] as const,
  recipes: ["admin-recipes"] as const,
  recipe: (productId: string) => ["admin-recipes", productId] as const
};

const tabs = [
  { id: "brands", label: "Brands", icon: Database },
  { id: "menu", label: "Menu", icon: Utensils },
  { id: "recipes", label: "Recipes", icon: BookOpen },
  { id: "stations", label: "Stations", icon: ChefHat }
] as const satisfies readonly {
  readonly id: TabId;
  readonly label: string;
  readonly icon: LucideIcon;
}[];

export function App() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<TabId>("brands");
  const [email, setEmail] = useState("manager@darkkitchen.local");
  const [password, setPassword] = useState(demoPassword);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

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
  const [recipeDraft, setRecipeDraft] = useState<RecipeDraft | null>(null);
  const [routeDrafts, setRouteDrafts] = useState<Record<string, string>>({});

  const sessionQuery = useQuery({
    queryKey: queryKeys.session,
    queryFn: ({ signal }) => getCurrentSession(signal),
    retry: false,
    staleTime: 60_000
  });

  const session = sessionQuery.data ?? null;
  const isSignedIn = session !== null;
  const isManager = session?.roles?.includes("Manager") ?? false;
  const canWrite = isSignedIn && isManager;

  const catalogQuery = useQuery({
    queryKey: queryKeys.catalog,
    queryFn: ({ signal }) => getCatalogSnapshot(signal),
    enabled: isSignedIn,
    staleTime: 30_000
  });

  const catalog = catalogQuery.data ?? emptyCatalogSnapshot;
  const { brands, categories, products, ingredients, stations } = catalog;
  const effectiveRecipeProductId = selectedRecipeProductId.length > 0
    ? selectedRecipeProductId
    : products[0]?.id ?? "";

  const recipeQuery = useQuery({
    queryKey: queryKeys.recipe(effectiveRecipeProductId),
    queryFn: ({ signal }) => getRecipe(effectiveRecipeProductId, signal),
    enabled: isSignedIn && effectiveRecipeProductId.length > 0,
    staleTime: 30_000
  });

  const loginMutation = useMutation({
    mutationFn: (request: { readonly email: string; readonly password: string }) => (
      loginAdmin(request.email, request.password)
    ),
    onSuccess: current => {
      queryClient.setQueryData<Session | null>(queryKeys.session, current);
      void queryClient.invalidateQueries({ queryKey: queryKeys.catalog });
    }
  });

  const logoutMutation = useMutation({
    mutationFn: logoutAdmin,
    onSettled: () => {
      queryClient.setQueryData<Session | null>(queryKeys.session, null);
      queryClient.removeQueries({ queryKey: queryKeys.catalog });
      queryClient.removeQueries({ queryKey: queryKeys.recipes });
    }
  });

  const catalogMutation = useMutation({
    mutationFn: (request: CatalogMutationRequest) => request.action(),
    onSuccess: async (_data, request) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.catalog });
      if (request.recipeProductId !== undefined) {
        await queryClient.invalidateQueries({ queryKey: queryKeys.recipe(request.recipeProductId) });
      }
    }
  });

  const busy = loginMutation.isPending || logoutMutation.isPending || catalogMutation.isPending;

  const brandById = useMemo(() => new Map(brands.map(brand => [brand.id, brand])), [brands]);
  const categoryById = useMemo(() => new Map(categories.map(category => [category.id, category])), [categories]);
  const activeBrands = useMemo(() => brands.filter(brand => brand.isActive), [brands]);
  const activeCategories = useMemo(() => categories.filter(category => category.isActive), [categories]);
  const activeProducts = useMemo(() => products.filter(product => product.isActive), [products]);
  const activeStations = useMemo(() => stations.filter(station => station.isActive), [stations]);
  const activeIngredients = useMemo(() => ingredients.filter(ingredient => ingredient.isActive), [ingredients]);
  const loadedRecipeRows = useMemo(() => recipeQuery.data?.items.map(item => ({
    ingredientId: item.ingredientId,
    quantity: String(item.quantity)
  })) ?? [], [recipeQuery.data]);
  const recipeRows = recipeDraft?.productId === effectiveRecipeProductId
    ? recipeDraft.rows
    : loadedRecipeRows;
  const productCategories = productForm.brandId.length > 0
    ? activeCategories.filter(category => category.brandId === productForm.brandId)
    : activeCategories;

  const metrics = [
    { label: "Brands", value: brands.length, detail: `${activeBrands.length} active`, icon: Database },
    { label: "Products", value: products.length, detail: `${activeProducts.length} live`, icon: Package },
    { label: "Ingredients", value: ingredients.length, detail: `${activeIngredients.length} usable`, icon: Boxes },
    { label: "Stations", value: stations.length, detail: `${activeStations.length} online`, icon: ChefHat }
  ] as const;

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

  async function runCatalogMutation(request: CatalogMutationRequest, success: string) {
    if (!requireManager()) {
      return;
    }

    clearMessages();
    try {
      await catalogMutation.mutateAsync(request);
      setNotice(success);
    } catch (mutationError) {
      setError(errorMessage(mutationError, "Request failed."));
    }
  }

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearMessages();
    try {
      await loginMutation.mutateAsync({ email, password });
      setNotice("Signed in.");
    } catch (loginError) {
      setError(errorMessage(loginError, "Login failed."));
    }
  }

  async function handleLogout() {
    clearMessages();
    try {
      await logoutMutation.mutateAsync();
    } catch {
      queryClient.setQueryData<Session | null>(queryKeys.session, null);
    } finally {
      setActiveTab("brands");
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
    await runCatalogMutation({
      action: async () => {
        await saveBrandRequest(editingBrandId, {
          name: brandForm.name,
          description: brandForm.description,
          logoUrl: brandForm.logoUrl,
          isActive: brandForm.isActive
        });
        setEditingBrandId(null);
        setBrandForm(emptyBrandForm);
      }
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
    await runCatalogMutation({
      action: async () => {
        await saveCategoryRequest(editingCategoryId, {
          brandId: categoryForm.brandId,
          name: categoryForm.name,
          sortOrder: Number(categoryForm.sortOrder),
          isActive: categoryForm.isActive
        });
        setEditingCategoryId(null);
        setCategoryForm(emptyCategoryForm);
      }
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
    await runCatalogMutation({
      action: async () => {
        await saveProductRequest(editingProductId, {
          brandId: productForm.brandId,
          categoryId: productForm.categoryId,
          name: productForm.name,
          description: productForm.description,
          price: Number(productForm.price),
          currency: productForm.currency
        });
        setEditingProductId(null);
        setProductForm(emptyProductForm);
      }
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
    await runCatalogMutation({
      action: async () => {
        await saveIngredientRequest(editingIngredientId, {
          name: ingredientForm.name,
          unit: ingredientForm.unit,
          isActive: ingredientForm.isActive
        });
        setEditingIngredientId(null);
        setIngredientForm(emptyIngredientForm);
      }
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
    await runCatalogMutation({
      action: async () => {
        await saveStationRequest(editingStationId, {
          code: stationForm.code,
          name: stationForm.name,
          displayColor: stationForm.displayColor,
          isActive: stationForm.isActive
        });
        setEditingStationId(null);
        setStationForm(emptyStationForm);
      }
    }, "Station saved.");
  }

  async function saveRoute(productId: string) {
    await runCatalogMutation({
      action: () => saveProductStationRoute(productId, routeDrafts[productId] ?? "")
    }, "Route saved.");
  }

  async function saveRecipe(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (effectiveRecipeProductId.length === 0) {
      setError("Select product first.");
      return;
    }

    await runCatalogMutation({
      action: () => saveRecipeRequest(effectiveRecipeProductId, {
        items: recipeRows.map(row => ({
          ingredientId: row.ingredientId,
          quantity: Number(row.quantity)
        }))
      }),
      recipeProductId: effectiveRecipeProductId
    }, "Recipe saved.");
  }

  function addRecipeRow() {
    updateRecipeRows(current => [
      ...current,
      { ingredientId: activeIngredients[0]?.id ?? "", quantity: "1" }
    ]);
  }

  function updateRecipeRows(updater: (current: RecipeDraftRow[]) => RecipeDraftRow[]) {
    setRecipeDraft({
      productId: effectiveRecipeProductId,
      rows: updater(recipeRows)
    });
  }

  if (sessionQuery.isPending) {
    return <LoadingScreen />;
  }

  if (!isSignedIn) {
    return (
      <main className="login-screen">
        <section className="login-visual" aria-label="Dark Kitchen operations">
          <p className="brand-mark">Dark Kitchen</p>
          <h1>{appMetadata.name}</h1>
          <p>{appMetadata.description}</p>
          <div className="login-stats" aria-label="Admin scope">
            <MetricCard icon={Database} label="Catalog" value="4" detail="core modules" />
            <MetricCard icon={ShieldCheck} label="Access" value="2" detail="roles" />
          </div>
        </section>
        <section className="login-panel" aria-labelledby="login-title">
          <div className="panel-kicker">
            <KeyRound aria-hidden="true" />
            <span>Secure workspace</span>
          </div>
          <h2 id="login-title">Sign in</h2>
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
                <UserRound aria-hidden="true" />
                Manager demo
              </button>
              <button type="button" onClick={() => pickDemoAccount("operator@darkkitchen.local")}>
                <ShieldCheck aria-hidden="true" />
                Operator demo
              </button>
            </div>
            {error !== null && <Message tone="error">{error}</Message>}
            <ActionButton icon={Power} label="Sign in" type="submit" variant="primary" disabled={busy} />
          </form>
        </section>
      </main>
    );
  }

  return (
    <main className="admin-layout">
      <aside className="sidebar">
        <div className="brand-block">
          <p className="brand-mark">Dark Kitchen</p>
          <h1>{appMetadata.name}</h1>
          <p>{appMetadata.context}</p>
        </div>

        <nav className="side-nav" aria-label="Admin sections">
          {tabs.map(tab => {
            const Icon = tab.icon;
            return (
              <button
                key={tab.id}
                type="button"
                className={tab.id === activeTab ? "is-selected" : ""}
                onClick={() => setActiveTab(tab.id)}
              >
                <Icon aria-hidden="true" />
                <span>{tab.label}</span>
                <span className="nav-count">{tabCount(tab.id, catalog)}</span>
              </button>
            );
          })}
        </nav>

        <div className="sidebar-card">
          <span className={apiConfigured ? "system-dot is-ok" : "system-dot"} />
          <div>
            <strong>API</strong>
            <p>{apiConfigured ? "Configured" : "Relative URL"}</p>
          </div>
        </div>
      </aside>

      <section className="admin-main">
        <header className="command-bar">
          <div>
            <p className="section-kicker">Catalog command center</p>
            <h2>Menu, recipes and stations</h2>
          </div>
          <div className="session-box">
            <span>{session.email}</span>
            <span className={isManager ? "pill is-manager" : "pill"}>
              {isManager ? "Manager" : "Operator"}
            </span>
            <ActionButton icon={LogOut} label="Logout" onClick={() => void handleLogout()} disabled={busy} />
          </div>
        </header>

        <section className="metric-grid" aria-label="Catalog summary">
          {metrics.map(metric => (
            <MetricCard
              key={metric.label}
              icon={metric.icon}
              label={metric.label}
              value={String(metric.value)}
              detail={metric.detail}
            />
          ))}
        </section>

        {(error !== null || notice !== null || !canWrite || catalogQuery.isError) && (
          <div className="message-stack">
            {!canWrite && <Message>Operator read-only mode</Message>}
            {notice !== null && <Message tone="success">{notice}</Message>}
            {error !== null && <Message tone="error">{error}</Message>}
            {catalogQuery.isError && <Message tone="error">{errorMessage(catalogQuery.error, "Catalog load failed.")}</Message>}
          </div>
        )}

        {catalogQuery.isPending ? (
          <section className="workspace">
            <div className="empty-state">
              <RefreshCw aria-hidden="true" />
              <strong>Loading catalog</strong>
              <span>Preparing workspace.</span>
            </div>
          </section>
        ) : (
          <>
            {activeTab === "brands" && renderBrands()}
            {activeTab === "menu" && renderMenu()}
            {activeTab === "recipes" && renderRecipes()}
            {activeTab === "stations" && renderStations()}
          </>
        )}
      </section>
    </main>
  );

  function renderBrands() {
    return (
      <section className="workspace" aria-labelledby="brands-title">
        <PanelIntro
          id="brands-title"
          title="Brands"
          detail={`${brands.length} records`}
          icon={Database}
        />
        <div className="split-layout">
          <form className="editor-panel" onSubmit={event => void saveBrand(event)}>
            <PanelTitle>{editingBrandId === null ? "New brand" : "Edit brand"}</PanelTitle>
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
            <FormActions>
              <ActionButton icon={Save} label="Save" type="submit" variant="primary" disabled={!canWrite || busy} />
              <ActionButton
                icon={X}
                label="Clear"
                onClick={() => {
                  setEditingBrandId(null);
                  setBrandForm(emptyBrandForm);
                }}
              />
            </FormActions>
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
                {brands.length === 0 && <EmptyRow colSpan={4} label="No brands yet." />}
                {brands.map(brand => (
                  <tr key={brand.id}>
                    <td className="strong-cell">{brand.name}</td>
                    <td><StatusBadge isActive={brand.isActive} /></td>
                    <td>{textOrDash(brand.description)}</td>
                    <td className="row-actions">
                      <ActionButton icon={Pencil} label="Edit" onClick={() => editBrand(brand)} />
                      <ActionButton
                        icon={Power}
                        label="Deactivate"
                        variant="danger"
                        disabled={!canWrite || !brand.isActive || busy}
                        onClick={() => void runCatalogMutation({ action: () => deactivateBrand(brand.id) }, "Brand deactivated.")}
                      />
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
        <PanelIntro id="menu-title" title="Menu" detail={`${products.length} products`} icon={Utensils} />
        <div className="two-column">
          <form className="editor-panel" onSubmit={event => void saveCategory(event)}>
            <PanelTitle>{editingCategoryId === null ? "New category" : "Edit category"}</PanelTitle>
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
            <FormActions>
              <ActionButton icon={Save} label="Save" type="submit" variant="primary" disabled={!canWrite || busy} />
              <ActionButton
                icon={X}
                label="Clear"
                onClick={() => {
                  setEditingCategoryId(null);
                  setCategoryForm(emptyCategoryForm);
                }}
              />
            </FormActions>
          </form>

          <form className="editor-panel" onSubmit={event => void saveProduct(event)}>
            <PanelTitle>{editingProductId === null ? "New product" : "Edit product"}</PanelTitle>
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
            <FormActions>
              <ActionButton icon={Save} label="Save" type="submit" variant="primary" disabled={!canWrite || busy} />
              <ActionButton
                icon={X}
                label="Clear"
                onClick={() => {
                  setEditingProductId(null);
                  setProductForm(emptyProductForm);
                }}
              />
            </FormActions>
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
              {products.length === 0 && <EmptyRow colSpan={8} label="No products yet." />}
              {products.map(product => (
                <tr key={product.id}>
                  <td className="strong-cell">{product.name}</td>
                  <td>{brandById.get(product.brandId)?.name ?? "-"}</td>
                  <td>{categoryById.get(product.categoryId)?.name ?? "-"}</td>
                  <td>{product.price.toFixed(2)} {product.currency}</td>
                  <td><StatusBadge isActive={product.isActive} /></td>
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
                    <ActionButton icon={Pencil} label="Edit" onClick={() => editProduct(product)} />
                    <ActionButton icon={Route} label="Route" disabled={!canWrite || busy} onClick={() => void saveRoute(product.id)} />
                    <ActionButton
                      icon={CircleCheck}
                      label="Activate"
                      disabled={!canWrite || product.isActive || busy}
                      onClick={() => void runCatalogMutation({ action: () => activateProductRequest(product.id) }, "Product activated.")}
                    />
                    <ActionButton
                      icon={Power}
                      label="Deactivate"
                      variant="danger"
                      disabled={!canWrite || !product.isActive || busy}
                      onClick={() => void runCatalogMutation({ action: () => deactivateProduct(product.id) }, "Product deactivated.")}
                    />
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
              {categories.length === 0 && <EmptyRow colSpan={5} label="No categories yet." />}
              {categories.map(category => (
                <tr key={category.id}>
                  <td className="strong-cell">{category.name}</td>
                  <td>{brandById.get(category.brandId)?.name ?? "-"}</td>
                  <td>{category.sortOrder}</td>
                  <td><StatusBadge isActive={category.isActive} /></td>
                  <td className="row-actions">
                    <ActionButton icon={Pencil} label="Edit" onClick={() => editCategory(category)} />
                    <ActionButton
                      icon={Power}
                      label="Deactivate"
                      variant="danger"
                      disabled={!canWrite || !category.isActive || busy}
                      onClick={() => void runCatalogMutation({ action: () => deactivateCategory(category.id) }, "Category deactivated.")}
                    />
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
        <PanelIntro id="recipes-title" title="Recipes" detail={`${ingredients.length} ingredients`} icon={BookOpen} />
        <div className="two-column">
          <form className="editor-panel" onSubmit={event => void saveIngredient(event)}>
            <PanelTitle>{editingIngredientId === null ? "New ingredient" : "Edit ingredient"}</PanelTitle>
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
            <FormActions>
              <ActionButton icon={Save} label="Save" type="submit" variant="primary" disabled={!canWrite || busy} />
              <ActionButton
                icon={X}
                label="Clear"
                onClick={() => {
                  setEditingIngredientId(null);
                  setIngredientForm(emptyIngredientForm);
                }}
              />
            </FormActions>
          </form>

          <form className="editor-panel recipe-editor" onSubmit={event => void saveRecipe(event)}>
            <PanelTitle>{selectedProduct?.name ?? "Recipe"}</PanelTitle>
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
              {recipeQuery.isFetching && <span className="muted-line">Refreshing recipe...</span>}
              {recipeRows.map((row, index) => (
                <div className="recipe-row" key={`${row.ingredientId}-${index.toString()}`}>
                  <select
                    value={row.ingredientId}
                    disabled={!canWrite}
                    aria-label={`Ingredient ${index + 1}`}
                    onChange={event => updateRecipeRows(current => current.map((item, itemIndex) => (
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
                    onChange={event => updateRecipeRows(current => current.map((item, itemIndex) => (
                      itemIndex === index ? { ...item, quantity: event.currentTarget.value } : item
                    )))}
                  />
                  <ActionButton
                    icon={X}
                    label="Remove"
                    disabled={!canWrite}
                    onClick={() => updateRecipeRows(current => current.filter((_row, itemIndex) => itemIndex !== index))}
                  />
                </div>
              ))}
            </div>
            <FormActions>
              <ActionButton icon={Plus} label="Add item" disabled={!canWrite || activeIngredients.length === 0} onClick={addRecipeRow} />
              <ActionButton
                icon={Save}
                label="Save recipe"
                type="submit"
                variant="primary"
                disabled={!canWrite || busy || effectiveRecipeProductId.length === 0}
              />
            </FormActions>
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
              {ingredients.length === 0 && <EmptyRow colSpan={4} label="No ingredients yet." />}
              {ingredients.map(ingredient => (
                <tr key={ingredient.id}>
                  <td className="strong-cell">{ingredient.name}</td>
                  <td>{ingredient.unit}</td>
                  <td><StatusBadge isActive={ingredient.isActive} /></td>
                  <td className="row-actions">
                    <ActionButton icon={Pencil} label="Edit" onClick={() => editIngredient(ingredient)} />
                    <ActionButton
                      icon={Power}
                      label="Deactivate"
                      variant="danger"
                      disabled={!canWrite || !ingredient.isActive || busy}
                      onClick={() => void runCatalogMutation({ action: () => deactivateIngredient(ingredient.id) }, "Ingredient deactivated.")}
                    />
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
        <PanelIntro id="stations-title" title="Stations" detail={`${stations.length} records`} icon={ChefHat} />
        <div className="split-layout">
          <form className="editor-panel" onSubmit={event => void saveStation(event)}>
            <PanelTitle>{editingStationId === null ? "New station" : "Edit station"}</PanelTitle>
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
            <FormActions>
              <ActionButton icon={Save} label="Save" type="submit" variant="primary" disabled={!canWrite || busy} />
              <ActionButton
                icon={X}
                label="Clear"
                onClick={() => {
                  setEditingStationId(null);
                  setStationForm(emptyStationForm);
                }}
              />
            </FormActions>
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
                {stations.length === 0 && <EmptyRow colSpan={5} label="No stations yet." />}
                {stations.map(station => (
                  <tr key={station.id}>
                    <td className="strong-cell">{station.code}</td>
                    <td>{station.name}</td>
                    <td>
                      <span className="color-chip" style={{ backgroundColor: station.displayColor }} />
                      {station.displayColor}
                    </td>
                    <td><StatusBadge isActive={station.isActive} /></td>
                    <td className="row-actions">
                      <ActionButton icon={Pencil} label="Edit" onClick={() => editStation(station)} />
                      <ActionButton
                        icon={Power}
                        label="Deactivate"
                        variant="danger"
                        disabled={!canWrite || !station.isActive || busy}
                        onClick={() => void runCatalogMutation({ action: () => deactivateStation(station.id) }, "Station deactivated.")}
                      />
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

function LoadingScreen() {
  return (
    <main className="login-screen is-loading">
      <section className="login-panel" aria-labelledby="loading-title">
        <p className="brand-mark">Dark Kitchen</p>
        <h1 id="loading-title">{appMetadata.name}</h1>
        <div className="empty-state">
          <RefreshCw aria-hidden="true" />
          <strong>Checking session</strong>
        </div>
      </section>
    </main>
  );
}

function ActionButton({
  icon: Icon,
  label,
  onClick,
  disabled = false,
  type = "button",
  variant = "quiet"
}: {
  readonly icon: LucideIcon;
  readonly label: string;
  readonly onClick?: () => void;
  readonly disabled?: boolean;
  readonly type?: "button" | "submit";
  readonly variant?: "primary" | "quiet" | "danger";
}) {
  return (
    <button
      type={type}
      className={`action-button is-${variant}`}
      disabled={disabled}
      onClick={onClick}
    >
      <Icon aria-hidden="true" />
      <span>{label}</span>
    </button>
  );
}

function MetricCard({
  icon: Icon,
  label,
  value,
  detail
}: {
  readonly icon: LucideIcon;
  readonly label: string;
  readonly value: string;
  readonly detail: string;
}) {
  return (
    <article className="metric-card">
      <Icon aria-hidden="true" />
      <div>
        <span>{label}</span>
        <strong>{value}</strong>
        <small>{detail}</small>
      </div>
    </article>
  );
}

function PanelIntro({
  id,
  title,
  detail,
  icon: Icon
}: {
  readonly id: string;
  readonly title: string;
  readonly detail: string;
  readonly icon: LucideIcon;
}) {
  return (
    <div className="panel-intro">
      <div>
        <span className="panel-icon"><Icon aria-hidden="true" /></span>
        <h2 id={id}>{title}</h2>
      </div>
      <span>{detail}</span>
    </div>
  );
}

function PanelTitle({ children }: { readonly children: ReactNode }) {
  return (
    <h3 className="panel-title">
      <ClipboardList aria-hidden="true" />
      {children}
    </h3>
  );
}

function FormActions({ children }: { readonly children: ReactNode }) {
  return <div className="form-actions">{children}</div>;
}

function Message({
  children,
  tone = "info"
}: {
  readonly children: ReactNode;
  readonly tone?: "info" | "success" | "error";
}) {
  const Icon = tone === "success" ? CircleCheck : tone === "error" ? CircleAlert : ShieldCheck;
  return (
    <p className={`message is-${tone}`}>
      <Icon aria-hidden="true" />
      <span>{children}</span>
    </p>
  );
}

function StatusBadge({ isActive }: { readonly isActive: boolean }) {
  return (
    <span className={isActive ? "status is-active" : "status"}>
      {isActive ? "Active" : "Inactive"}
    </span>
  );
}

function EmptyRow({ colSpan, label }: { readonly colSpan: number; readonly label: string }) {
  return (
    <tr>
      <td colSpan={colSpan}>
        <div className="empty-row">
          <Database aria-hidden="true" />
          <span>{label}</span>
        </div>
      </td>
    </tr>
  );
}

function tabCount(tabId: TabId, catalog: CatalogSnapshot) {
  switch (tabId) {
    case "brands":
      return catalog.brands.length;
    case "menu":
      return catalog.products.length;
    case "recipes":
      return catalog.ingredients.length;
    case "stations":
      return catalog.stations.length;
  }
}

function textOrDash(value: string | null | undefined) {
  return value === null || value === undefined || value.length === 0 ? "-" : value;
}

function errorMessage(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback;
}
