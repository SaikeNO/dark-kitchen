import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { BookOpen, CircleCheck, Package, Pencil, Plus, Power, Save, Search, X } from "lucide-react";
import { type FormEvent, useMemo, useState } from "react";
import { toast } from "sonner";
import { errorMessage } from "../../api/http";
import { queryKeys } from "../../api/queryKeys";
import { Button } from "../../shared/ui/Button";
import { EmptyState } from "../../shared/ui/EmptyState";
import { Checkbox, Field, Select, TextInput } from "../../shared/ui/Fields";
import { PageHeader } from "../../shared/ui/PageHeader";
import { Panel } from "../../shared/ui/Panel";
import { StatusBadge } from "../../shared/ui/StatusBadge";
import type { Product } from "../menu/menuTypes";
import { deactivateIngredient, getRecipe, saveIngredient, saveRecipe } from "./recipesApi";
import type { Ingredient } from "./recipeTypes";

type StatusFilter = "all" | "active" | "inactive";

interface RecipeDraftRow {
  readonly ingredientId: string;
  readonly quantity: string;
}

interface RecipeDraft {
  readonly productId: string;
  readonly rows: RecipeDraftRow[];
}

const emptyIngredientForm = { name: "", unit: "g", isActive: true };

export function RecipeProductsPage({
  products,
  ingredients,
  canWrite
}: {
  readonly products: Product[];
  readonly ingredients: Ingredient[];
  readonly canWrite: boolean;
}) {
  const queryClient = useQueryClient();
  const [selectedProductId, setSelectedProductId] = useState("");
  const [recipeDraft, setRecipeDraft] = useState<RecipeDraft | null>(null);
  const [productSearch, setProductSearch] = useState("");

  const selectedProduct = products.find(product => product.id === selectedProductId);
  const ingredientById = useMemo(() => new Map(ingredients.map(ingredient => [ingredient.id, ingredient])), [ingredients]);
  const filteredProducts = useMemo(() => {
    const normalizedSearch = productSearch.trim().toLowerCase();

    return normalizedSearch.length === 0
      ? products
      : products.filter(product => product.name.toLowerCase().includes(normalizedSearch));
  }, [productSearch, products]);

  const recipeQuery = useQuery({
    queryKey: queryKeys.recipe(selectedProductId),
    queryFn: ({ signal }) => getRecipe(selectedProductId, signal),
    enabled: selectedProductId.length > 0,
    staleTime: 30_000
  });

  const loadedRows = useMemo(() => recipeQuery.data?.items.map(item => ({
    ingredientId: item.ingredientId,
    quantity: String(item.quantity)
  })) ?? [], [recipeQuery.data]);

  const recipeRows = recipeDraft?.productId === selectedProductId ? recipeDraft.rows : loadedRows;

  const recipeMutation = useMutation({
    mutationFn: () => saveRecipe(selectedProductId, {
      items: recipeRows
        .filter(row => row.ingredientId.length > 0)
        .map(row => ({
          ingredientId: row.ingredientId,
          quantity: Number(row.quantity)
        }))
    }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.recipe(selectedProductId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.products })
      ]);
      setRecipeDraft(null);
      toast.success("Zapisano recepturę.");
    },
    onError: error => toast.error(errorMessage(error))
  });

  function requireWrite() {
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return false;
    }

    return true;
  }

  function selectProduct(productId: string) {
    setSelectedProductId(productId);
    setRecipeDraft(null);
  }

  function updateRows(updater: (rows: RecipeDraftRow[]) => RecipeDraftRow[]) {
    if (selectedProductId.length === 0) {
      return;
    }

    setRecipeDraft({ productId: selectedProductId, rows: updater(recipeRows) });
  }

  function submitRecipe(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!requireWrite()) {
      return;
    }

    if (selectedProductId.length === 0) {
      toast.error("Wybierz produkt.");
      return;
    }

    if (recipeRows.some(row => row.ingredientId.length === 0 || !Number.isFinite(Number(row.quantity)) || Number(row.quantity) <= 0)) {
      toast.error("Uzupełnij składnik i ilość większą od zera.");
      return;
    }

    recipeMutation.mutate();
  }

  return (
    <div className="grid gap-4">
      <PageHeader title="Receptury produktów" description="Najpierw wybierz produkt, potem zbuduj jego recepturę." />

      <div className="grid gap-4 xl:grid-cols-[320px_minmax(0,1fr)]">
        <Panel title="Produkty">
          <Field label="Szukaj produktu">
            <div className="relative">
              <Search aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-zinc-400" />
              <TextInput className="pl-9" value={productSearch} onChange={event => setProductSearch(event.currentTarget.value)} />
            </div>
          </Field>
          <div className="mt-4 grid gap-2">
            {products.length === 0 ? (
              <EmptyState icon={Package} title="Brak produktów" description="Dodaj produkt w Menu, potem wróć do receptury." />
            ) : filteredProducts.length === 0 ? (
              <EmptyState icon={Search} title="Brak wyników" />
            ) : (
              filteredProducts.map(product => (
                <button
                  key={product.id}
                  type="button"
                  className={`rounded-lg border p-3 text-left transition-colors ${product.id === selectedProductId
                    ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-950/30"
                    : "border-zinc-200 hover:bg-zinc-50 dark:border-zinc-800 dark:hover:bg-zinc-900"}`}
                  onClick={() => selectProduct(product.id)}
                >
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="font-bold text-zinc-950 dark:text-zinc-50">{product.name}</span>
                    <StatusBadge isActive={product.isActive} />
                  </div>
                  <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">{product.recipeItemCount} pozycji receptury</p>
                </button>
              ))
            )}
          </div>
        </Panel>

        <Panel title={selectedProduct?.name ?? "Receptura produktu"}>
          {selectedProductId.length === 0 ? (
            <EmptyState icon={Package} title="Wybierz produkt" description="Po wyborze produktu pobrana zostanie jego receptura." />
          ) : recipeQuery.isPending ? (
            <EmptyState icon={BookOpen} title="Ładowanie receptury" description="Pobieranie składników produktu..." />
          ) : (
            <form className="grid gap-4" onSubmit={submitRecipe}>
              {recipeQuery.isFetching && <p className="text-sm text-zinc-500 dark:text-zinc-400">Odświeżanie receptury...</p>}
              {recipeRows.length === 0 && (
                <EmptyState icon={BookOpen} title="Brak pozycji" description="Dodaj pierwszy składnik do receptury." />
              )}
              {recipeRows.map((row, index) => {
                const ingredient = ingredientById.get(row.ingredientId);

                return (
                  <div key={`${row.ingredientId}-${index.toString()}`} className="grid gap-2 rounded-lg border border-zinc-200 p-3 dark:border-zinc-800 sm:grid-cols-[1fr_9rem_6rem_auto] sm:items-end">
                    <Field label="Składnik">
                      <Select
                        value={row.ingredientId}
                        disabled={!canWrite}
                        onChange={event => updateRows(rows => rows.map((item, itemIndex) => (
                          itemIndex === index ? { ...item, ingredientId: event.currentTarget.value } : item
                        )))}
                      >
                        <option value="">Wybierz składnik</option>
                        {ingredients.map(item => (
                          <option key={item.id} value={item.id}>{item.name}{item.isActive ? "" : " (nieaktywny)"}</option>
                        ))}
                      </Select>
                    </Field>
                    <Field label="Ilość">
                      <TextInput
                        inputMode="decimal"
                        type="number"
                        min="0"
                        step="0.001"
                        value={row.quantity}
                        disabled={!canWrite}
                        onChange={event => updateRows(rows => rows.map((item, itemIndex) => (
                          itemIndex === index ? { ...item, quantity: event.currentTarget.value } : item
                        )))}
                      />
                    </Field>
                    <div className="rounded-md border border-zinc-200 bg-zinc-50 px-3 py-2 text-sm font-semibold text-zinc-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300">
                      {ingredient?.unit ?? "-"}
                    </div>
                    <Button type="button" icon={X} disabled={!canWrite} onClick={() => updateRows(rows => rows.filter((_row, itemIndex) => itemIndex !== index))}>
                      Usuń
                    </Button>
                  </div>
                );
              })}
              <div className="grid gap-2 sm:grid-cols-2">
                <Button
                  type="button"
                  icon={Plus}
                  disabled={!canWrite || ingredients.length === 0}
                  onClick={() => updateRows(rows => [...rows, { ingredientId: ingredients.find(ingredient => ingredient.isActive)?.id ?? ingredients[0]?.id ?? "", quantity: "1" }])}
                >
                  Dodaj składnik
                </Button>
                <Button type="submit" variant="primary" icon={Save} disabled={!canWrite || recipeMutation.isPending}>
                  Zapisz recepturę
                </Button>
              </div>
            </form>
          )}
        </Panel>
      </div>
    </div>
  );
}

export function IngredientsPage({
  ingredients,
  canWrite
}: {
  readonly ingredients: Ingredient[];
  readonly canWrite: boolean;
}) {
  const queryClient = useQueryClient();
  const [ingredientForm, setIngredientForm] = useState(emptyIngredientForm);
  const [editingIngredientId, setEditingIngredientId] = useState<string | null>(null);
  const [editorOpen, setEditorOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const filteredIngredients = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return ingredients.filter(ingredient => {
      const matchesSearch = normalizedSearch.length === 0
        || ingredient.name.toLowerCase().includes(normalizedSearch)
        || ingredient.unit.toLowerCase().includes(normalizedSearch);
      const matchesStatus = statusFilter === "all"
        || (statusFilter === "active" && ingredient.isActive)
        || (statusFilter === "inactive" && !ingredient.isActive);

      return matchesSearch && matchesStatus;
    });
  }, [ingredients, search, statusFilter]);

  const mutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<unknown>; readonly success: string }) => request.action(),
    onSuccess: async (_data, request) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.ingredients });
      toast.success(request.success);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function requireWrite() {
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return false;
    }

    return true;
  }

  function openNewIngredient() {
    setEditingIngredientId(null);
    setIngredientForm(emptyIngredientForm);
    setEditorOpen(true);
  }

  function closeEditor() {
    setEditingIngredientId(null);
    setIngredientForm(emptyIngredientForm);
    setEditorOpen(false);
  }

  function submitIngredient(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!requireWrite()) {
      return;
    }

    if (ingredientForm.name.trim().length === 0) {
      toast.error("Podaj nazwę składnika.");
      return;
    }

    if (ingredientForm.unit.trim().length === 0) {
      toast.error("Podaj jednostkę.");
      return;
    }

    mutation.mutate({
      action: () => saveIngredient(editingIngredientId, {
        name: ingredientForm.name.trim(),
        unit: ingredientForm.unit.trim(),
        isActive: ingredientForm.isActive
      }),
      success: editingIngredientId === null ? "Dodano składnik." : "Zapisano składnik."
    }, {
      onSuccess: closeEditor
    });
  }

  function editIngredient(ingredient: Ingredient) {
    setEditingIngredientId(ingredient.id);
    setIngredientForm({
      name: ingredient.name,
      unit: ingredient.unit,
      isActive: ingredient.isActive
    });
    setEditorOpen(true);
  }

  function changeIngredientStatus(ingredient: Ingredient, isActive: boolean) {
    if (!requireWrite()) {
      return;
    }

    mutation.mutate({
      action: () => isActive
        ? saveIngredient(ingredient.id, {
          name: ingredient.name,
          unit: ingredient.unit,
          isActive: true
        })
        : deactivateIngredient(ingredient.id),
      success: isActive ? "Aktywowano składnik." : "Dezaktywowano składnik."
    });
  }

  return (
    <div className="grid gap-4">
      <PageHeader
        title="Składniki"
        description="Osobny widok do dodawania, edycji i filtrowania składników receptur."
        actions={<Button icon={Plus} variant="primary" disabled={!canWrite} onClick={openNewIngredient}>Dodaj składnik</Button>}
      />

      <Panel>
        <div className="grid gap-3 sm:grid-cols-[1fr_10rem]">
          <Field label="Szukaj">
            <div className="relative">
              <Search aria-hidden="true" className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-zinc-400" />
              <TextInput className="pl-9" value={search} placeholder="Nazwa lub jednostka" onChange={event => setSearch(event.currentTarget.value)} />
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

      {editorOpen && (
        <Panel title={editingIngredientId === null ? "Nowy składnik" : "Edycja składnika"}>
          <form className="grid gap-4" onSubmit={submitIngredient}>
            <div className="grid gap-4 sm:grid-cols-[1fr_10rem]">
              <Field label="Nazwa">
                <TextInput value={ingredientForm.name} disabled={!canWrite} onChange={event => setIngredientForm({ ...ingredientForm, name: event.currentTarget.value })} />
              </Field>
              <Field label="Jednostka">
                <TextInput value={ingredientForm.unit} disabled={!canWrite} onChange={event => setIngredientForm({ ...ingredientForm, unit: event.currentTarget.value })} />
              </Field>
            </div>
            <Checkbox label="Aktywny" checked={ingredientForm.isActive} disabled={!canWrite} onChange={event => setIngredientForm({ ...ingredientForm, isActive: event.currentTarget.checked })} />
            <div className="grid gap-2 sm:flex">
              <Button type="submit" variant="primary" icon={Save} disabled={!canWrite || mutation.isPending}>
                Zapisz składnik
              </Button>
              <Button type="button" icon={X} onClick={closeEditor}>
                Zamknij
              </Button>
            </div>
          </form>
        </Panel>
      )}

      <Panel title="Lista składników">
        {ingredients.length === 0 ? (
          <EmptyState icon={BookOpen} title="Brak składników" description="Dodaj pierwszy składnik, potem użyj go w recepturze." />
        ) : filteredIngredients.length === 0 ? (
          <EmptyState icon={Search} title="Brak wyników" />
        ) : (
          <div className="grid gap-3 md:grid-cols-2">
            {filteredIngredients.map(ingredient => (
              <article key={ingredient.id} className="rounded-lg border border-zinc-200 p-3 dark:border-zinc-800">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="font-bold text-zinc-950 dark:text-zinc-50">{ingredient.name}</h3>
                  <StatusBadge isActive={ingredient.isActive} />
                </div>
                <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">Jednostka: {ingredient.unit}</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  <Button icon={Pencil} onClick={() => editIngredient(ingredient)}>
                    Edytuj
                  </Button>
                  {ingredient.isActive ? (
                    <Button icon={Power} variant="danger" disabled={!canWrite || mutation.isPending} onClick={() => changeIngredientStatus(ingredient, false)}>
                      Dezaktywuj
                    </Button>
                  ) : (
                    <Button icon={CircleCheck} disabled={!canWrite || mutation.isPending} onClick={() => changeIngredientStatus(ingredient, true)}>
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
