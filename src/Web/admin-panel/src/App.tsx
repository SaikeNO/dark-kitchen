import { useQuery } from "@tanstack/react-query";
import {
  BookOpen,
  Building2,
  ChefHat,
  Layers,
  Loader2,
  Menu,
  Package,
  Utensils,
  type LucideIcon
} from "lucide-react";
import { useEffect, useState } from "react";
import {
  Navigate,
  NavLink,
  Outlet,
  Route,
  Routes,
  useLocation,
  useOutletContext,
  useParams
} from "react-router-dom";
import { Toaster, toast } from "sonner";
import { apiConfigured, errorMessage } from "./api/http";
import { queryKeys } from "./api/queryKeys";
import { appMetadata } from "./appMetadata";
import { LoginScreen } from "./features/auth/LoginScreen";
import { SessionMenu } from "./features/auth/SessionMenu";
import { getCurrentSession } from "./features/auth/authApi";
import type { Session } from "./features/auth/authTypes";
import { BrandsPage } from "./features/brands/BrandsPage";
import { listBrands } from "./features/brands/brandsApi";
import type { Brand } from "./features/brands/brandTypes";
import {
  MenuCategoriesPage,
  MenuCategoryFormPage,
  MenuProductFormPage,
  MenuProductsPage
} from "./features/menu/MenuPage";
import { listCategories, listProducts } from "./features/menu/menuApi";
import { IngredientsPage, RecipeProductsPage } from "./features/recipes/RecipesPage";
import { listIngredients } from "./features/recipes/recipesApi";
import { StationsPage } from "./features/stations/StationsPage";
import { listStations } from "./features/stations/stationsApi";
import { Button } from "./shared/ui/Button";
import { cn } from "./shared/ui/cn";
import "./styles.css";

type ThemeMode = "light" | "dark";

interface AdminOutletContext {
  readonly canWrite: boolean;
  readonly brands: Brand[];
  readonly brandsPending: boolean;
}

const mainSections = [
  { path: "/brands", label: "Marki", icon: Building2 },
  { path: "/menu", label: "Menu", icon: Utensils },
  { path: "/recipes", label: "Receptury", icon: BookOpen },
  { path: "/stations", label: "Stacje", icon: ChefHat }
] as const;

export function App() {
  const [themeMode, setThemeMode] = useState<ThemeMode>(() => initialTheme());
  const darkMode = themeMode === "dark";

  useEffect(() => {
    document.documentElement.classList.toggle("dark", darkMode);
    localStorage.setItem("dark-kitchen-admin-theme", themeMode);
  }, [darkMode, themeMode]);

  function toggleTheme() {
    setThemeMode(current => current === "dark" ? "light" : "dark");
  }

  return (
    <>
      <Routes>
        <Route element={<AuthGate darkMode={darkMode} onToggleTheme={toggleTheme} />}>
          <Route index element={<Navigate to="/brands" replace />} />
          <Route path="brands" element={<BrandsRoute />} />
          <Route path="menu">
            <Route index element={<MenuIndexRoute />} />
            <Route path=":brandId" element={<BrandMenuIndexRoute />} />
            <Route path=":brandId/products" element={<MenuProductsRoute />} />
            <Route path=":brandId/products/new" element={<MenuProductFormRoute />} />
            <Route path=":brandId/products/:productId/edit" element={<MenuProductFormRoute />} />
            <Route path=":brandId/categories" element={<MenuCategoriesRoute />} />
            <Route path=":brandId/categories/new" element={<MenuCategoryFormRoute />} />
            <Route path=":brandId/categories/:categoryId/edit" element={<MenuCategoryFormRoute />} />
          </Route>
          <Route path="recipes">
            <Route index element={<Navigate to="/recipes/products" replace />} />
            <Route path="products" element={<RecipeProductsRoute />} />
            <Route path="ingredients" element={<IngredientsRoute />} />
          </Route>
          <Route path="stations" element={<StationsRoute />} />
          <Route path="*" element={<Navigate to="/brands" replace />} />
        </Route>
      </Routes>
      <Toaster richColors position="bottom-right" theme={darkMode ? "dark" : "light"} />
    </>
  );
}

function AuthGate({
  darkMode,
  onToggleTheme
}: {
  readonly darkMode: boolean;
  readonly onToggleTheme: () => void;
}) {
  const sessionQuery = useQuery({
    queryKey: queryKeys.session,
    queryFn: ({ signal }) => getCurrentSession(signal),
    retry: false,
    staleTime: 60_000
  });

  const session = sessionQuery.data ?? null;

  if (sessionQuery.isPending) {
    return <LoadingScreen />;
  }

  if (session === null) {
    return <LoginScreen darkMode={darkMode} onToggleTheme={onToggleTheme} />;
  }

  return <AdminLayout session={session} darkMode={darkMode} onToggleTheme={onToggleTheme} />;
}

function AdminLayout({
  session,
  darkMode,
  onToggleTheme
}: {
  readonly session: Session;
  readonly darkMode: boolean;
  readonly onToggleTheme: () => void;
}) {
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const location = useLocation();
  const canWrite = session.roles.includes("Manager");
  const activeTitle = mainSections.find(section => location.pathname.startsWith(section.path))?.label ?? "Panel";
  const brandsQuery = useQuery({
    queryKey: queryKeys.brands,
    queryFn: ({ signal }) => listBrands(signal),
    staleTime: 30_000
  });
  useQueryErrorToast(brandsQuery.error, "Nie udało się pobrać marek.");

  const brands = brandsQuery.data ?? [];

  return (
    <main className="min-h-screen bg-zinc-50 text-zinc-950 dark:bg-zinc-950 dark:text-zinc-50">
      <a className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-emerald-600 focus:px-3 focus:py-2 focus:text-sm focus:font-bold focus:text-white" href="#main-content">
        Przejdź do treści
      </a>
      <div className="grid min-h-screen lg:grid-cols-[260px_1fr]">
        <aside className="hidden border-r border-zinc-200 bg-white p-4 dark:border-zinc-800 dark:bg-zinc-900 lg:sticky lg:top-0 lg:block lg:h-screen lg:overflow-y-auto">
          <BrandBlock />
          <SectionNav brands={brands} brandsPending={brandsQuery.isPending} />
          <div className="mt-6 rounded-lg border border-zinc-200 p-3 text-sm text-zinc-600 dark:border-zinc-800 dark:text-zinc-400">
            API: <span className="font-bold text-zinc-900 dark:text-zinc-100">{apiConfigured ? "configured" : "relative URL"}</span>
          </div>
        </aside>

        <section className="min-w-0">
          <header className="sticky top-0 z-20 border-b border-zinc-200 bg-white/90 px-4 py-3 backdrop-blur dark:border-zinc-800 dark:bg-zinc-950/90">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex min-w-0 items-center gap-2">
                <Button className="lg:hidden" variant="ghost" icon={Menu} onClick={() => setMobileNavOpen(current => !current)}>
                  Menu
                </Button>
                <div className="min-w-0">
                  <p className="text-xs font-black uppercase tracking-normal text-emerald-700 dark:text-emerald-400">Dark Kitchen</p>
                  <h1 className="truncate text-xl font-black">{activeTitle}</h1>
                </div>
              </div>
              <SessionMenu session={session} darkMode={darkMode} onToggleTheme={onToggleTheme} />
            </div>
            {mobileNavOpen && (
              <div className="mt-3 max-h-[70vh] overflow-y-auto lg:hidden">
                <SectionNav brands={brands} brandsPending={brandsQuery.isPending} compact onNavigate={() => setMobileNavOpen(false)} />
              </div>
            )}
          </header>

          <div id="main-content" className="mx-auto grid max-w-6xl gap-4 p-4">
            {!canWrite && (
              <div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm font-semibold text-amber-900 dark:border-amber-900/60 dark:bg-amber-950/40 dark:text-amber-200">
                Tryb operatora: odczyt bez zapisu.
              </div>
            )}
            <Outlet context={{ canWrite, brands, brandsPending: brandsQuery.isPending } satisfies AdminOutletContext} />
          </div>
        </section>
      </div>
    </main>
  );
}

function BrandsRoute() {
  const { canWrite, brands, brandsPending } = useAdminContext();

  return brandsPending
    ? <LoadingPanel />
    : <BrandsPage brands={brands} canWrite={canWrite} />;
}

function MenuIndexRoute() {
  const { brands, brandsPending } = useAdminContext();

  if (brandsPending) {
    return <LoadingPanel />;
  }

  const firstBrand = brands.find(brand => brand.isActive) ?? brands[0];

  return firstBrand === undefined
    ? <NoDataPanel icon={Building2} title="Brak marek" description="Dodaj markę, potem zbuduj jej menu." />
    : <Navigate to={`/menu/${firstBrand.id}/products`} replace />;
}

function BrandMenuIndexRoute() {
  const { brandId } = useParams();

  return brandId === undefined
    ? <Navigate to="/menu" replace />
    : <Navigate to={`/menu/${brandId}/products`} replace />;
}

function MenuProductsRoute() {
  const { canWrite, brands, brandsPending } = useAdminContext();
  const { brandId } = useParams();
  const brand = brands.find(item => item.id === brandId);
  const categoriesQuery = useQuery({
    queryKey: queryKeys.categories,
    queryFn: ({ signal }) => listCategories(signal),
    staleTime: 30_000
  });
  const productsQuery = useQuery({
    queryKey: queryKeys.products,
    queryFn: ({ signal }) => listProducts(signal),
    staleTime: 30_000
  });

  useQueryErrorToast(categoriesQuery.error, "Nie udało się pobrać kategorii.");
  useQueryErrorToast(productsQuery.error, "Nie udało się pobrać produktów.");

  if (brandsPending || categoriesQuery.isPending || productsQuery.isPending) {
    return <LoadingPanel />;
  }

  if (brand === undefined) {
    return <Navigate to="/menu" replace />;
  }

  return (
    <MenuProductsPage
      brand={brand}
      categories={categoriesQuery.data ?? []}
      products={productsQuery.data ?? []}
      canWrite={canWrite}
    />
  );
}

function MenuProductFormRoute() {
  const { canWrite, brands, brandsPending } = useAdminContext();
  const { brandId, productId } = useParams();
  const brand = brands.find(item => item.id === brandId);
  const categoriesQuery = useQuery({
    queryKey: queryKeys.categories,
    queryFn: ({ signal }) => listCategories(signal),
    staleTime: 30_000
  });
  const productsQuery = useQuery({
    queryKey: queryKeys.products,
    queryFn: ({ signal }) => listProducts(signal),
    enabled: productId !== undefined,
    staleTime: 30_000
  });
  const stationsQuery = useQuery({
    queryKey: queryKeys.stations,
    queryFn: ({ signal }) => listStations(signal),
    staleTime: 30_000
  });

  useQueryErrorToast(categoriesQuery.error, "Nie udało się pobrać kategorii.");
  useQueryErrorToast(productsQuery.error, "Nie udało się pobrać produktów.");
  useQueryErrorToast(stationsQuery.error, "Nie udało się pobrać stacji.");

  if (brandsPending || categoriesQuery.isPending || stationsQuery.isPending || (productId !== undefined && productsQuery.isPending)) {
    return <LoadingPanel />;
  }

  if (brand === undefined) {
    return <Navigate to="/menu" replace />;
  }

  const product = productId === undefined
    ? undefined
    : (productsQuery.data ?? []).find(item => item.id === productId);

  if (productId !== undefined && product === undefined) {
    return <NoDataPanel icon={Package} title="Nie znaleziono produktu" description="Wróć do listy i wybierz istniejący produkt." />;
  }

  if (product !== undefined && product.brandId !== brand.id) {
    return <Navigate to={`/menu/${product.brandId}/products/${product.id}/edit`} replace />;
  }

  return (
    <MenuProductFormPage
      key={product?.id ?? `new-product-${brand.id}`}
      brand={brand}
      categories={categoriesQuery.data ?? []}
      stations={stationsQuery.data ?? []}
      product={product}
      canWrite={canWrite}
    />
  );
}

function MenuCategoriesRoute() {
  const { canWrite, brands, brandsPending } = useAdminContext();
  const { brandId } = useParams();
  const brand = brands.find(item => item.id === brandId);
  const categoriesQuery = useQuery({
    queryKey: queryKeys.categories,
    queryFn: ({ signal }) => listCategories(signal),
    staleTime: 30_000
  });
  useQueryErrorToast(categoriesQuery.error, "Nie udało się pobrać kategorii.");

  if (brandsPending || categoriesQuery.isPending) {
    return <LoadingPanel />;
  }

  if (brand === undefined) {
    return <Navigate to="/menu" replace />;
  }

  return <MenuCategoriesPage brand={brand} categories={categoriesQuery.data ?? []} canWrite={canWrite} />;
}

function MenuCategoryFormRoute() {
  const { canWrite, brands, brandsPending } = useAdminContext();
  const { brandId, categoryId } = useParams();
  const brand = brands.find(item => item.id === brandId);
  const categoriesQuery = useQuery({
    queryKey: queryKeys.categories,
    queryFn: ({ signal }) => listCategories(signal),
    staleTime: 30_000
  });
  useQueryErrorToast(categoriesQuery.error, "Nie udało się pobrać kategorii.");

  if (brandsPending || categoriesQuery.isPending) {
    return <LoadingPanel />;
  }

  if (brand === undefined) {
    return <Navigate to="/menu" replace />;
  }

  const category = categoryId === undefined
    ? undefined
    : (categoriesQuery.data ?? []).find(item => item.id === categoryId);

  if (categoryId !== undefined && category === undefined) {
    return <NoDataPanel icon={Layers} title="Nie znaleziono kategorii" description="Wróć do listy i wybierz istniejącą kategorię." />;
  }

  if (category !== undefined && category.brandId !== brand.id) {
    return <Navigate to={`/menu/${category.brandId}/categories/${category.id}/edit`} replace />;
  }

  return <MenuCategoryFormPage key={category?.id ?? `new-category-${brand.id}`} brand={brand} category={category} canWrite={canWrite} />;
}

function RecipeProductsRoute() {
  const { canWrite } = useAdminContext();
  const productsQuery = useQuery({
    queryKey: queryKeys.products,
    queryFn: ({ signal }) => listProducts(signal),
    staleTime: 30_000
  });
  const ingredientsQuery = useQuery({
    queryKey: queryKeys.ingredients,
    queryFn: ({ signal }) => listIngredients(signal),
    staleTime: 30_000
  });

  useQueryErrorToast(productsQuery.error, "Nie udało się pobrać produktów.");
  useQueryErrorToast(ingredientsQuery.error, "Nie udało się pobrać składników.");

  return productsQuery.isPending || ingredientsQuery.isPending
    ? <LoadingPanel />
    : <RecipeProductsPage products={productsQuery.data ?? []} ingredients={ingredientsQuery.data ?? []} canWrite={canWrite} />;
}

function IngredientsRoute() {
  const { canWrite } = useAdminContext();
  const ingredientsQuery = useQuery({
    queryKey: queryKeys.ingredients,
    queryFn: ({ signal }) => listIngredients(signal),
    staleTime: 30_000
  });
  useQueryErrorToast(ingredientsQuery.error, "Nie udało się pobrać składników.");

  return ingredientsQuery.isPending
    ? <LoadingPanel />
    : <IngredientsPage ingredients={ingredientsQuery.data ?? []} canWrite={canWrite} />;
}

function StationsRoute() {
  const { canWrite } = useAdminContext();
  const stationsQuery = useQuery({
    queryKey: queryKeys.stations,
    queryFn: ({ signal }) => listStations(signal),
    staleTime: 30_000
  });
  useQueryErrorToast(stationsQuery.error, "Nie udało się pobrać stacji.");

  return stationsQuery.isPending
    ? <LoadingPanel />
    : <StationsPage stations={stationsQuery.data ?? []} canWrite={canWrite} />;
}

function BrandBlock() {
  return (
    <div className="mb-6">
      <p className="text-xs font-black uppercase tracking-normal text-emerald-700 dark:text-emerald-400">Dark Kitchen</p>
      <h2 className="mt-1 text-2xl font-black tracking-tight">{appMetadata.name}</h2>
      <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">{appMetadata.context}</p>
    </div>
  );
}

function SectionNav({
  brands,
  brandsPending,
  compact = false,
  onNavigate
}: {
  readonly brands: Brand[];
  readonly brandsPending: boolean;
  readonly compact?: boolean;
  readonly onNavigate?: () => void;
}) {
  return (
    <nav className={cn("grid gap-2", compact && "rounded-lg border border-zinc-200 bg-white p-2 dark:border-zinc-800 dark:bg-zinc-900")} aria-label="Sekcje panelu">
      <NavItem to="/brands" icon={Building2} label="Marki" onNavigate={onNavigate} />

      <div className="grid gap-1">
        <NavItem to="/menu" icon={Utensils} label="Menu" onNavigate={onNavigate} />
        <div className="grid gap-1">
          {brandsPending && <p className="px-10 py-2 text-xs font-semibold text-zinc-500 dark:text-zinc-400">Ładowanie marek...</p>}
          {!brandsPending && brands.length === 0 && <p className="px-10 py-2 text-xs font-semibold text-zinc-500 dark:text-zinc-400">Brak marek</p>}
          {!brandsPending && brands.map(brand => (
            <NavItem
              key={brand.id}
              to={`/menu/${brand.id}`}
              label={brand.isActive ? brand.name : `${brand.name} (nieaktywna)`}
              child
              onNavigate={onNavigate}
            />
          ))}
        </div>
      </div>

      <div className="grid gap-1">
        <NavItem to="/recipes" icon={BookOpen} label="Receptury" onNavigate={onNavigate} />
        <div className="grid gap-1">
          <NavItem to="/recipes/products" label="Receptury produktów" icon={Package} child onNavigate={onNavigate} />
          <NavItem to="/recipes/ingredients" label="Składniki" icon={Layers} child onNavigate={onNavigate} />
        </div>
      </div>

      <NavItem to="/stations" icon={ChefHat} label="Stacje" onNavigate={onNavigate} />
    </nav>
  );
}

function NavItem({
  to,
  label,
  icon: Icon,
  child = false,
  onNavigate
}: {
  readonly to: string;
  readonly label: string;
  readonly icon?: LucideIcon;
  readonly child?: boolean;
  readonly onNavigate?: () => void;
}) {
  return (
    <NavLink
      to={to}
      onClick={onNavigate}
      className={({ isActive }) => cn(
        "flex items-center gap-2 rounded-md px-3 py-2 text-left font-bold transition-colors",
        child ? "ml-7 min-h-9 text-xs" : "min-h-11 text-sm",
        isActive
          ? "bg-emerald-600 text-white dark:bg-emerald-500 dark:text-zinc-950"
          : "text-zinc-700 hover:bg-zinc-100 dark:text-zinc-200 dark:hover:bg-zinc-800"
      )}
    >
      {Icon !== undefined && <Icon aria-hidden="true" className="size-4 shrink-0" />}
      <span className="min-w-0 truncate">{label}</span>
    </NavLink>
  );
}

function LoadingScreen() {
  return (
    <main className="grid min-h-screen place-items-center bg-zinc-50 p-4 dark:bg-zinc-950">
      <div className="rounded-lg border border-zinc-200 bg-white p-6 text-center shadow-sm dark:border-zinc-800 dark:bg-zinc-900">
        <Loader2 aria-hidden="true" className="mx-auto size-8 animate-spin text-emerald-600 dark:text-emerald-400" />
        <h1 className="mt-3 text-xl font-black text-zinc-950 dark:text-zinc-50">{appMetadata.name}</h1>
        <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">Sprawdzanie sesji...</p>
      </div>
    </main>
  );
}

function LoadingPanel() {
  return (
    <div className="grid min-h-64 place-items-center rounded-lg border border-zinc-200 bg-white p-8 dark:border-zinc-800 dark:bg-zinc-900">
      <div className="text-center">
        <Loader2 aria-hidden="true" className="mx-auto size-8 animate-spin text-emerald-600 dark:text-emerald-400" />
        <p className="mt-3 text-sm font-semibold text-zinc-600 dark:text-zinc-400">Ładowanie danych...</p>
      </div>
    </div>
  );
}

function NoDataPanel({
  icon: Icon,
  title,
  description
}: {
  readonly icon: LucideIcon;
  readonly title: string;
  readonly description: string;
}) {
  return (
    <div className="grid min-h-64 place-items-center rounded-lg border border-zinc-200 bg-white p-8 text-center dark:border-zinc-800 dark:bg-zinc-900">
      <Icon aria-hidden="true" className="mx-auto size-8 text-zinc-400" />
      <h2 className="mt-3 text-lg font-black text-zinc-950 dark:text-zinc-50">{title}</h2>
      <p className="mt-1 max-w-md text-sm text-zinc-600 dark:text-zinc-400">{description}</p>
    </div>
  );
}

function useAdminContext() {
  return useOutletContext<AdminOutletContext>();
}

function useQueryErrorToast(error: unknown, fallback: string) {
  useEffect(() => {
    if (error !== null && error !== undefined) {
      toast.error(errorMessage(error, fallback));
    }
  }, [error, fallback]);
}

function initialTheme(): ThemeMode {
  if (typeof localStorage === "undefined" || typeof window === "undefined") {
    return "light";
  }

  const saved = localStorage.getItem("dark-kitchen-admin-theme");
  if (saved === "light" || saved === "dark") {
    return saved;
  }

  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}
