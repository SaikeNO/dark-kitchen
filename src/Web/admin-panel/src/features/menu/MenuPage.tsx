import { useMutation, useQueryClient } from "@tanstack/react-query";
import { CircleCheck, ImageUp, Layers, Pencil, Plus, Power, Save, Search, Utensils, X } from "lucide-react";
import { type FormEvent, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { errorMessage } from "../../api/http";
import { queryKeys } from "../../api/queryKeys";
import { Button } from "../../shared/ui/Button";
import { EmptyState } from "../../shared/ui/EmptyState";
import { Checkbox, Field, Select, Textarea, TextInput } from "../../shared/ui/Fields";
import { PageHeader } from "../../shared/ui/PageHeader";
import { Panel } from "../../shared/ui/Panel";
import { StatusBadge } from "../../shared/ui/StatusBadge";
import type { Brand } from "../brands/brandTypes";
import type { Station } from "../stations/stationTypes";
import {
  activateProduct,
  deactivateCategory,
  deactivateProduct,
  saveCategory,
  saveProduct,
  saveProductStationRoute,
  uploadProductImage
} from "./menuApi";
import type { Category, Product } from "./menuTypes";

type StatusFilter = "all" | "active" | "inactive";

interface ProductFormState {
  readonly categoryId: string;
  readonly name: string;
  readonly description: string;
  readonly imageUrl: string;
  readonly price: string;
  readonly currency: string;
  readonly stationId: string;
}

interface CategoryFormState {
  readonly name: string;
  readonly sortOrder: string;
  readonly isActive: boolean;
}

export function MenuProductsPage({
  brand,
  categories,
  products,
  canWrite
}: {
  readonly brand: Brand;
  readonly categories: Category[];
  readonly products: Product[];
  readonly canWrite: boolean;
}) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const brandCategories = useMemo(
    () => categories.filter(category => category.brandId === brand.id),
    [brand.id, categories]
  );
  const categoryById = useMemo(
    () => new Map(categories.map(category => [category.id, category])),
    [categories]
  );
  const brandProducts = products.filter(product => product.brandId === brand.id);
  const filteredProducts = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return brandProducts.filter(product => {
      const matchesSearch = normalizedSearch.length === 0
        || product.name.toLowerCase().includes(normalizedSearch)
        || (product.description ?? "").toLowerCase().includes(normalizedSearch);
      const matchesCategory = categoryFilter.length === 0 || product.categoryId === categoryFilter;
      const matchesStatus = statusFilter === "all"
        || (statusFilter === "active" && product.isActive)
        || (statusFilter === "inactive" && !product.isActive);

      return matchesSearch && matchesCategory && matchesStatus;
    });
  }, [brandProducts, categoryFilter, search, statusFilter]);

  const mutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<unknown>; readonly success: string }) => request.action(),
    onSuccess: async (_data, request) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.products });
      toast.success(request.success);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function go(to: string) {
    void navigate(to);
  }

  function requireWrite() {
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return false;
    }

    return true;
  }

  function changeProductStatus(product: Product, isActive: boolean) {
    if (!requireWrite()) {
      return;
    }

    mutation.mutate({
      action: () => isActive ? activateProduct(product.id) : deactivateProduct(product.id),
      success: isActive ? "Aktywowano produkt." : "Dezaktywowano produkt."
    });
  }

  return (
    <div className="grid gap-4">
      <PageHeader
        title={`Menu: ${brand.name}`}
        description="Produkty są filtrowane w obrębie marki. Stację wybierzesz dopiero podczas dodawania lub edycji pozycji."
        actions={(
          <>
            <Button icon={Layers} onClick={() => go(`/menu/${brand.id}/categories`)}>
              Kategorie
            </Button>
            <Button icon={Plus} variant="primary" disabled={!canWrite} onClick={() => go(`/menu/${brand.id}/products/new`)}>
              Dodaj produkt
            </Button>
          </>
        )}
      />

      <Panel>
        <div className="grid gap-3 md:grid-cols-[1.5fr_1fr_10rem]">
          <Field label="Szukaj">
            <div className="relative">
              <Search aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-zinc-400" />
              <TextInput className="pl-9" value={search} placeholder="Nazwa lub opis" onChange={event => setSearch(event.currentTarget.value)} />
            </div>
          </Field>
          <Field label="Kategoria">
            <Select value={categoryFilter} onChange={event => setCategoryFilter(event.currentTarget.value)}>
              <option value="">Wszystkie</option>
              {brandCategories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
            </Select>
          </Field>
          <Field label="Status">
            <Select value={statusFilter} onChange={event => setStatusFilter(event.currentTarget.value as StatusFilter)}>
              <option value="all">Wszystkie</option>
              <option value="active">Aktywne</option>
              <option value="inactive">Nieaktywne</option>
            </Select>
          </Field>
        </div>
      </Panel>

      <Panel title="Produkty">
        {brandProducts.length === 0 ? (
          <EmptyState icon={Utensils} title="Brak produktów" description="Dodaj pierwszą pozycję menu dla tej marki." />
        ) : filteredProducts.length === 0 ? (
          <EmptyState icon={Search} title="Brak wyników" description="Zmień filtry lub wyczyść wyszukiwanie." />
        ) : (
          <div className="grid gap-3">
            {filteredProducts.map(product => (
              <article key={product.id} className="grid gap-3 rounded-lg border border-zinc-200 p-3 dark:border-zinc-800 lg:grid-cols-[1fr_auto] lg:items-center">
                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-2">
                    {product.imageUrl === null ? <ImageUp aria-hidden="true" className="size-4 text-zinc-400" /> : <img src={product.imageUrl} alt="" className="size-10 rounded-md object-cover" />}
                    <h3 className="truncate font-bold text-zinc-950 dark:text-zinc-50">{product.name}</h3>
                    <StatusBadge isActive={product.isActive} />
                  </div>
                  <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
                    {categoryById.get(product.categoryId)?.name ?? "Bez kategorii"} / {product.price.toFixed(2)} {product.currency}
                  </p>
                  <p className="mt-1 text-xs font-semibold text-zinc-500 dark:text-zinc-500">
                    Stacja: {product.stationCode ?? "brak"}
                  </p>
                </div>
                <div className="grid gap-2 sm:flex lg:justify-end">
                  <Button icon={Pencil} onClick={() => go(`/menu/${brand.id}/products/${product.id}/edit`)}>
                    Edytuj
                  </Button>
                  {product.isActive ? (
                    <Button icon={Power} variant="danger" disabled={!canWrite || mutation.isPending} onClick={() => changeProductStatus(product, false)}>
                      Dezaktywuj
                    </Button>
                  ) : (
                    <Button icon={CircleCheck} disabled={!canWrite || mutation.isPending} onClick={() => changeProductStatus(product, true)}>
                      Aktywuj
                    </Button>
                  )}
                </div>
              </article>
            ))}
          </div>
        )}
      </Panel>
    </div>
  );
}

export function MenuProductFormPage({
  brand,
  categories,
  stations,
  product,
  canWrite
}: {
  readonly brand: Brand;
  readonly categories: Category[];
  readonly stations: Station[];
  readonly product?: Product;
  readonly canWrite: boolean;
}) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const brandCategories = useMemo(
    () => categories.filter(category => category.brandId === brand.id),
    [brand.id, categories]
  );
  const firstCategoryId = brandCategories.find(category => category.isActive)?.id ?? brandCategories[0]?.id ?? "";
  const [form, setForm] = useState<ProductFormState>(() => productToForm(product, firstCategoryId));
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: async () => {
      const saved = await saveProduct(product?.id ?? null, {
        brandId: brand.id,
        categoryId: form.categoryId,
        name: form.name.trim(),
        description: form.description.trim(),
        imageUrl: form.imageUrl.trim(),
        price: Number(form.price),
        currency: form.currency.trim().toUpperCase()
      });
      const originalStationId = product?.stationId ?? "";
      const shouldSaveRoute = product === undefined
        ? form.stationId.length > 0
        : form.stationId !== originalStationId;

      if (shouldSaveRoute) {
        await saveProductStationRoute(saved.id, form.stationId);
      }

      return saved;
    },
    onSuccess: async saved => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.products });
      toast.success(product === undefined ? "Dodano produkt." : "Zapisano produkt.");
      void navigate(`/menu/${saved.brandId}/products`);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function submitProduct(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    if (form.name.trim().length === 0) {
      toast.error("Podaj nazwę produktu.");
      return;
    }

    if (form.categoryId.length === 0) {
      toast.error("Wybierz kategorię.");
      return;
    }

    if (!Number.isFinite(Number(form.price)) || Number(form.price) < 0) {
      toast.error("Podaj poprawną cenę.");
      return;
    }

    mutation.mutate();
  }

  async function uploadImage(file: File | undefined) {
    if (file === undefined) {
      return;
    }

    clearImagePreview();
    const previewUrl = URL.createObjectURL(file);
    setImagePreviewUrl(previewUrl);
    try {
      const uploaded = await uploadProductImage(file);
      setForm(current => ({ ...current, imageUrl: uploaded.url }));
      toast.success("Wgrano zdjęcie produktu.");
    } catch (error) {
      toast.error(errorMessage(error));
    }
  }

  function clearImagePreview() {
    setImagePreviewUrl(current => {
      if (current !== null) {
        URL.revokeObjectURL(current);
      }

      return null;
    });
  }

  return (
    <div className="grid gap-4">
      <PageHeader
        title={product === undefined ? "Nowy produkt" : "Edycja produktu"}
        description={`Marka: ${brand.name}. Routing do stacji ustawiasz tutaj, nie na liście produktów.`}
        actions={<Button icon={X} onClick={() => { void navigate(`/menu/${brand.id}/products`); }}>Wróć do listy</Button>}
      />

      <Panel>
        <form className="grid gap-4" onSubmit={submitProduct}>
          <div className="rounded-lg border border-zinc-200 bg-zinc-50 p-3 text-sm dark:border-zinc-800 dark:bg-zinc-900">
            <span className="font-semibold text-zinc-600 dark:text-zinc-400">Marka</span>
            <p className="mt-1 font-bold text-zinc-950 dark:text-zinc-50">{brand.name}</p>
          </div>

          <Field label="Kategoria">
            <Select value={form.categoryId} disabled={!canWrite || brandCategories.length === 0} onChange={event => setForm({ ...form, categoryId: event.currentTarget.value })}>
              <option value="">Wybierz kategorię</option>
              {brandCategories.map(category => (
                <option key={category.id} value={category.id}>
                  {category.name}{category.isActive ? "" : " (nieaktywna)"}
                </option>
              ))}
            </Select>
          </Field>

          <Field label="Nazwa">
            <TextInput value={form.name} disabled={!canWrite} onChange={event => setForm({ ...form, name: event.currentTarget.value })} />
          </Field>

          <Field label="Opis">
            <Textarea value={form.description} disabled={!canWrite} onChange={event => setForm({ ...form, description: event.currentTarget.value })} />
          </Field>

          <Field label="Zdjęcie URL">
            <TextInput value={form.imageUrl} disabled={!canWrite} onChange={event => setForm({ ...form, imageUrl: event.currentTarget.value })} />
          </Field>

          <Field label="Wgraj zdjęcie">
            <input
              type="file"
              accept="image/png,image/jpeg,image/webp"
              disabled={!canWrite}
              onChange={event => { void uploadImage(event.currentTarget.files?.[0]); }}
            />
          </Field>

          {form.imageUrl.length > 0 && (
            <div className="overflow-hidden rounded-lg border border-zinc-200 bg-zinc-50 dark:border-zinc-800 dark:bg-zinc-900">
              <img src={imagePreviewUrl ?? form.imageUrl} alt="" className="h-44 w-full object-cover" />
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-[1fr_8rem]">
            <Field label="Cena">
              <TextInput inputMode="decimal" type="number" min="0" step="0.01" value={form.price} disabled={!canWrite} onChange={event => setForm({ ...form, price: event.currentTarget.value })} />
            </Field>
            <Field label="Waluta">
              <TextInput value={form.currency} disabled={!canWrite} onChange={event => setForm({ ...form, currency: event.currentTarget.value })} />
            </Field>
          </div>

          <Field label="Stacja">
            <Select value={form.stationId} disabled={!canWrite} onChange={event => setForm({ ...form, stationId: event.currentTarget.value })}>
              <option value="">Brak stacji</option>
              {stations.map(station => (
                <option key={station.id} value={station.id}>
                  {station.code} - {station.name}{station.isActive ? "" : " (nieaktywna)"}
                </option>
              ))}
            </Select>
          </Field>

          {brandCategories.length === 0 && (
            <div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm font-semibold text-amber-900 dark:border-amber-900/60 dark:bg-amber-950/40 dark:text-amber-200">
              Najpierw dodaj kategorię dla tej marki.
            </div>
          )}

          <div className="grid gap-2 sm:flex">
            <Button type="submit" variant="primary" icon={Save} disabled={!canWrite || mutation.isPending || brandCategories.length === 0}>
              Zapisz produkt
            </Button>
            <Button type="button" icon={X} onClick={() => { void navigate(`/menu/${brand.id}/products`); }}>
              Anuluj
            </Button>
          </div>
        </form>
      </Panel>
    </div>
  );
}

export function MenuCategoriesPage({
  brand,
  categories,
  canWrite
}: {
  readonly brand: Brand;
  readonly categories: Category[];
  readonly canWrite: boolean;
}) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const brandCategories = categories.filter(category => category.brandId === brand.id);
  const filteredCategories = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return brandCategories.filter(category => {
      const matchesSearch = normalizedSearch.length === 0 || category.name.toLowerCase().includes(normalizedSearch);
      const matchesStatus = statusFilter === "all"
        || (statusFilter === "active" && category.isActive)
        || (statusFilter === "inactive" && !category.isActive);

      return matchesSearch && matchesStatus;
    });
  }, [brandCategories, search, statusFilter]);

  const mutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<unknown>; readonly success: string }) => request.action(),
    onSuccess: async (_data, request) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.categories });
      toast.success(request.success);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function go(to: string) {
    void navigate(to);
  }

  function requireWrite() {
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return false;
    }

    return true;
  }

  function changeCategoryStatus(category: Category, isActive: boolean) {
    if (!requireWrite()) {
      return;
    }

    mutation.mutate({
      action: () => isActive
        ? saveCategory(category.id, {
          brandId: category.brandId,
          name: category.name,
          sortOrder: category.sortOrder,
          isActive: true
        })
        : deactivateCategory(category.id),
      success: isActive ? "Aktywowano kategorię." : "Dezaktywowano kategorię."
    });
  }

  return (
    <div className="grid gap-4">
      <PageHeader
        title={`Kategorie: ${brand.name}`}
        description="Kategorie są osobnym widokiem, bez mieszania z listą produktów."
        actions={(
          <>
            <Button icon={Utensils} onClick={() => go(`/menu/${brand.id}/products`)}>
              Produkty
            </Button>
            <Button icon={Plus} variant="primary" disabled={!canWrite} onClick={() => go(`/menu/${brand.id}/categories/new`)}>
              Dodaj kategorię
            </Button>
          </>
        )}
      />

      <Panel>
        <div className="grid gap-3 sm:grid-cols-[1fr_10rem]">
          <Field label="Szukaj">
            <div className="relative">
              <Search aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-zinc-400" />
              <TextInput className="pl-9" value={search} placeholder="Nazwa kategorii" onChange={event => setSearch(event.currentTarget.value)} />
            </div>
          </Field>
          <Field label="Status">
            <Select value={statusFilter} onChange={event => setStatusFilter(event.currentTarget.value as StatusFilter)}>
              <option value="all">Wszystkie</option>
              <option value="active">Aktywne</option>
              <option value="inactive">Nieaktywne</option>
            </Select>
          </Field>
        </div>
      </Panel>

      <Panel title="Kategorie">
        {brandCategories.length === 0 ? (
          <EmptyState icon={Layers} title="Brak kategorii" description="Dodaj kategorię, aby tworzyć produkty w tej marce." />
        ) : filteredCategories.length === 0 ? (
          <EmptyState icon={Search} title="Brak wyników" />
        ) : (
          <div className="grid gap-3 md:grid-cols-2">
            {filteredCategories.map(category => (
              <article key={category.id} className="rounded-lg border border-zinc-200 p-3 dark:border-zinc-800">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="font-bold text-zinc-950 dark:text-zinc-50">{category.name}</h3>
                  <StatusBadge isActive={category.isActive} />
                </div>
                <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">Kolejność: {category.sortOrder}</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  <Button icon={Pencil} onClick={() => go(`/menu/${brand.id}/categories/${category.id}/edit`)}>
                    Edytuj
                  </Button>
                  {category.isActive ? (
                    <Button icon={Power} variant="danger" disabled={!canWrite || mutation.isPending} onClick={() => changeCategoryStatus(category, false)}>
                      Dezaktywuj
                    </Button>
                  ) : (
                    <Button icon={CircleCheck} disabled={!canWrite || mutation.isPending} onClick={() => changeCategoryStatus(category, true)}>
                      Aktywuj
                    </Button>
                  )}
                </div>
              </article>
            ))}
          </div>
        )}
      </Panel>
    </div>
  );
}

export function MenuCategoryFormPage({
  brand,
  category,
  canWrite
}: {
  readonly brand: Brand;
  readonly category?: Category;
  readonly canWrite: boolean;
}) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [form, setForm] = useState<CategoryFormState>(() => categoryToForm(category));

  const mutation = useMutation({
    mutationFn: () => saveCategory(category?.id ?? null, {
      brandId: brand.id,
      name: form.name.trim(),
      sortOrder: Number(form.sortOrder),
      isActive: form.isActive
    }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.categories });
      toast.success(category === undefined ? "Dodano kategorię." : "Zapisano kategorię.");
      void navigate(`/menu/${brand.id}/categories`);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function submitCategory(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    if (form.name.trim().length === 0) {
      toast.error("Podaj nazwę kategorii.");
      return;
    }

    if (!Number.isFinite(Number(form.sortOrder))) {
      toast.error("Podaj poprawną kolejność.");
      return;
    }

    mutation.mutate();
  }

  return (
    <div className="grid gap-4">
      <PageHeader
        title={category === undefined ? "Nowa kategoria" : "Edycja kategorii"}
        description={`Marka: ${brand.name}. Kategorie mają własny ekran edycji.`}
        actions={<Button icon={X} onClick={() => { void navigate(`/menu/${brand.id}/categories`); }}>Wróć do kategorii</Button>}
      />

      <Panel>
        <form className="grid gap-4" onSubmit={submitCategory}>
          <div className="rounded-lg border border-zinc-200 bg-zinc-50 p-3 text-sm dark:border-zinc-800 dark:bg-zinc-900">
            <span className="font-semibold text-zinc-600 dark:text-zinc-400">Marka</span>
            <p className="mt-1 font-bold text-zinc-950 dark:text-zinc-50">{brand.name}</p>
          </div>

          <Field label="Nazwa">
            <TextInput value={form.name} disabled={!canWrite} onChange={event => setForm({ ...form, name: event.currentTarget.value })} />
          </Field>

          <Field label="Kolejność">
            <TextInput inputMode="numeric" type="number" value={form.sortOrder} disabled={!canWrite} onChange={event => setForm({ ...form, sortOrder: event.currentTarget.value })} />
          </Field>

          <Checkbox label="Aktywna" checked={form.isActive} disabled={!canWrite} onChange={event => setForm({ ...form, isActive: event.currentTarget.checked })} />

          <div className="grid gap-2 sm:flex">
            <Button type="submit" variant="primary" icon={Save} disabled={!canWrite || mutation.isPending}>
              Zapisz kategorię
            </Button>
            <Button type="button" icon={X} onClick={() => { void navigate(`/menu/${brand.id}/categories`); }}>
              Anuluj
            </Button>
          </div>
        </form>
      </Panel>
    </div>
  );
}

function productToForm(product: Product | undefined, fallbackCategoryId: string): ProductFormState {
  return {
    categoryId: product?.categoryId ?? fallbackCategoryId,
    name: product?.name ?? "",
    description: product?.description ?? "",
    imageUrl: product?.imageUrl ?? "",
    price: product?.price.toString() ?? "0.00",
    currency: product?.currency ?? "PLN",
    stationId: product?.stationId ?? ""
  };
}

function categoryToForm(category: Category | undefined): CategoryFormState {
  return {
    name: category?.name ?? "",
    sortOrder: category?.sortOrder.toString() ?? "10",
    isActive: category?.isActive ?? true
  };
}
