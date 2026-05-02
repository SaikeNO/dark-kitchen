import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, Boxes, Loader2, PackagePlus, RefreshCw, SlidersHorizontal, type LucideIcon } from "lucide-react";
import { type FormEvent, useMemo, useState } from "react";
import { Toaster, toast } from "sonner";
import { apiConfigured, errorMessage } from "./api/http";
import { queryKeys } from "./api/queryKeys";
import { adjustInventoryItem, listInventoryItems, listShortages, recordDelivery } from "./inventoryApi";
import type { InventoryItem } from "./inventoryTypes";

type ViewMode = "all" | "shortages";
const emptyItems: InventoryItem[] = [];

export function App() {
  const queryClient = useQueryClient();
  const [viewMode, setViewMode] = useState<ViewMode>("all");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [deliveryQuantity, setDeliveryQuantity] = useState("");
  const [adjustOnHand, setAdjustOnHand] = useState("");
  const [adjustMin, setAdjustMin] = useState("");
  const query = useQuery({
    queryKey: viewMode === "shortages" ? queryKeys.inventoryShortages : queryKeys.inventoryItems,
    queryFn: ({ signal }) => viewMode === "shortages" ? listShortages(signal) : listInventoryItems(signal),
    staleTime: 15_000
  });

  const items = query.data;
  const visibleItems = items ?? emptyItems;
  const selectedItem = visibleItems.find(item => item.ingredientId === selectedId) ?? visibleItems[0];
  const activeItemId = selectedId ?? selectedItem?.ingredientId;
  const adjustmentOnHandValue = adjustOnHand.length > 0 ? adjustOnHand : selectedItem === undefined ? "" : String(selectedItem.onHandQuantity);
  const adjustmentMinValue = adjustMin.length > 0 ? adjustMin : selectedItem === undefined ? "" : String(selectedItem.minSafetyLevel);
  const shortageCount = visibleItems.filter(item => item.isBelowSafetyLevel).length;
  const reorderTotal = visibleItems.reduce((sum, item) => sum + item.reorderQuantity, 0);

  const mutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<InventoryItem>; readonly success: string }) => request.action(),
    onSuccess: async (_data, request) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.inventoryItems }),
        queryClient.invalidateQueries({ queryKey: queryKeys.inventoryShortages })
      ]);
      toast.success(request.success);
    },
    onError: error => toast.error(errorMessage(error))
  });

  const sortedItems = useMemo(() => [...visibleItems].sort((left, right) => {
    if (left.isBelowSafetyLevel !== right.isBelowSafetyLevel) {
      return left.isBelowSafetyLevel ? -1 : 1;
    }

    return left.name.localeCompare(right.name, "pl");
  }), [visibleItems]);

  function refresh() {
    void Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.inventoryItems }),
      queryClient.invalidateQueries({ queryKey: queryKeys.inventoryShortages })
    ]);
  }

  function submitDelivery(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (selectedItem === undefined) {
      return;
    }

    const quantity = Number(deliveryQuantity);
    if (!Number.isFinite(quantity) || quantity <= 0) {
      toast.error("Podaj dodatnia dostawe.");
      return;
    }

    mutation.mutate({
      action: () => recordDelivery(selectedItem.ingredientId, { quantity }),
      success: "Dostawa zapisana."
    });
  }

  function submitAdjustment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (selectedItem === undefined) {
      return;
    }

    const onHandQuantity = Number(adjustmentOnHandValue);
    const minSafetyLevel = Number(adjustmentMinValue);
    if (!Number.isFinite(onHandQuantity) || onHandQuantity < selectedItem.reservedQuantity) {
      toast.error("Stan fizyczny nie moze byc nizszy niz rezerwacje.");
      return;
    }

    if (!Number.isFinite(minSafetyLevel) || minSafetyLevel < 0) {
      toast.error("Minimum musi byc liczba dodatnia lub zerem.");
      return;
    }

    mutation.mutate({
      action: () => adjustInventoryItem(selectedItem.ingredientId, { onHandQuantity, minSafetyLevel }),
      success: "Korekta zapisana."
    });
  }

  return (
    <main className="min-h-screen bg-slate-50">
      <header className="sticky top-0 z-20 border-b border-slate-200 bg-white/95 px-4 py-3 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-3">
          <div className="min-w-0">
            <p className="text-xs font-black uppercase text-emerald-700">Dark Kitchen</p>
            <h1 className="truncate text-xl font-black">Magazyn</h1>
          </div>
          <button
            type="button"
            className="inline-flex min-h-10 items-center gap-2 rounded-md border border-slate-200 bg-white px-3 text-sm font-bold text-slate-800"
            onClick={refresh}
          >
            <RefreshCw aria-hidden="true" className="size-4" />
            <span>Odswiez</span>
          </button>
        </div>
      </header>

      <div className="mx-auto grid max-w-6xl gap-4 p-4">
        <section className="grid gap-3 sm:grid-cols-3">
          <Metric label="Pozycje" value={String(visibleItems.length)} />
          <Metric label="Braki" value={String(shortageCount)} tone={shortageCount > 0 ? "warning" : "ok"} />
          <Metric label="Do zamowienia" value={formatQuantity(reorderTotal)} />
        </section>

        <section className="flex gap-2 rounded-lg border border-slate-200 bg-white p-2">
          <ViewButton active={viewMode === "all"} icon={Boxes} label="Stany" onClick={() => setViewMode("all")} />
          <ViewButton active={viewMode === "shortages"} icon={AlertTriangle} label="Braki" onClick={() => setViewMode("shortages")} />
        </section>

        {query.isPending ? (
          <StateMessage icon={Loader2} title="Ladowanie stanow" spin />
        ) : query.isError ? (
          <StateMessage icon={AlertTriangle} title="Nie udalo sie pobrac danych" />
        ) : sortedItems.length === 0 ? (
          <StateMessage icon={Boxes} title={viewMode === "shortages" ? "Brak brakow" : "Brak pozycji magazynu"} />
        ) : (
          <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_360px]">
            <section className="grid gap-3">
              {sortedItems.map(item => (
                <button
                  key={item.ingredientId}
                  type="button"
                  className={`rounded-lg border bg-white p-4 text-left transition-colors ${item.ingredientId === activeItemId
                    ? "border-emerald-500 ring-2 ring-emerald-100"
                    : "border-slate-200 hover:border-slate-300"}`}
                  onClick={() => {
                    setSelectedId(item.ingredientId);
                    setAdjustOnHand(String(item.onHandQuantity));
                    setAdjustMin(String(item.minSafetyLevel));
                    setDeliveryQuantity("");
                  }}
                >
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <div className="min-w-0">
                      <h2 className="truncate text-base font-black text-slate-950">{item.name}</h2>
                      <p className="text-sm font-semibold text-slate-500">{item.unit}</p>
                    </div>
                    {item.isBelowSafetyLevel && (
                      <span className="inline-flex items-center gap-1 rounded-md bg-amber-100 px-2 py-1 text-xs font-black text-amber-800">
                        <AlertTriangle aria-hidden="true" className="size-3" />
                        Brak
                      </span>
                    )}
                  </div>
                  <div className="mt-3 grid grid-cols-2 gap-2 text-sm sm:grid-cols-4">
                    <Quantity label="Dostepne" value={item.availableQuantity} />
                    <Quantity label="Fizyczne" value={item.onHandQuantity} />
                    <Quantity label="Rezerwacje" value={item.reservedQuantity} />
                    <Quantity label="Minimum" value={item.minSafetyLevel} />
                  </div>
                </button>
              ))}
            </section>

            {selectedItem !== undefined && (
              <aside className="h-fit rounded-lg border border-slate-200 bg-white p-4 lg:sticky lg:top-20">
                <h2 className="text-lg font-black text-slate-950">{selectedItem.name}</h2>
                <p className="mt-1 text-sm font-semibold text-slate-500">
                  Dostepne: {formatQuantity(selectedItem.availableQuantity)} {selectedItem.unit}
                </p>
                {selectedItem.isBelowSafetyLevel && (
                  <div className="mt-3 rounded-md border border-amber-200 bg-amber-50 p-3 text-sm font-bold text-amber-900">
                    Do zamowienia: {formatQuantity(selectedItem.reorderQuantity)} {selectedItem.unit}
                  </div>
                )}

                <form className="mt-4 grid gap-3" onSubmit={submitDelivery}>
                  <label className="grid gap-1 text-sm font-bold text-slate-700">
                    Dostawa ({selectedItem.unit})
                    <input
                      className="min-h-11 rounded-md border border-slate-300 px-3 text-base"
                      inputMode="decimal"
                      type="number"
                      min="0"
                      step="0.001"
                      value={deliveryQuantity}
                      onChange={event => setDeliveryQuantity(event.currentTarget.value)}
                    />
                  </label>
                  <button
                    type="submit"
                    disabled={mutation.isPending}
                    className="inline-flex min-h-11 items-center justify-center gap-2 rounded-md bg-emerald-600 px-3 text-sm font-black text-white disabled:opacity-50"
                  >
                    <PackagePlus aria-hidden="true" className="size-4" />
                    <span>Dodaj dostawe</span>
                  </button>
                </form>

                <form className="mt-5 grid gap-3" onSubmit={submitAdjustment}>
                  <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-1">
                    <label className="grid gap-1 text-sm font-bold text-slate-700">
                      Stan fizyczny
                      <input
                        className="min-h-11 rounded-md border border-slate-300 px-3 text-base"
                        inputMode="decimal"
                        type="number"
                        min="0"
                        step="0.001"
                        value={adjustmentOnHandValue}
                        onChange={event => setAdjustOnHand(event.currentTarget.value)}
                      />
                    </label>
                    <label className="grid gap-1 text-sm font-bold text-slate-700">
                      Minimum
                      <input
                        className="min-h-11 rounded-md border border-slate-300 px-3 text-base"
                        inputMode="decimal"
                        type="number"
                        min="0"
                        step="0.001"
                        value={adjustmentMinValue}
                        onChange={event => setAdjustMin(event.currentTarget.value)}
                      />
                    </label>
                  </div>
                  <button
                    type="submit"
                    disabled={mutation.isPending}
                    className="inline-flex min-h-11 items-center justify-center gap-2 rounded-md border border-slate-300 bg-white px-3 text-sm font-black text-slate-900 disabled:opacity-50"
                  >
                    <SlidersHorizontal aria-hidden="true" className="size-4" />
                    <span>Zapisz korekte</span>
                  </button>
                </form>
              </aside>
            )}
          </div>
        )}

        <p className="text-xs font-semibold text-slate-500">
          API: {apiConfigured ? "configured" : "relative URL"}
        </p>
      </div>
      <Toaster richColors position="bottom-right" />
    </main>
  );
}

function Metric({ label, value, tone }: { readonly label: string; readonly value: string; readonly tone?: "ok" | "warning" }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4">
      <p className="text-xs font-black uppercase text-slate-500">{label}</p>
      <p className={`mt-1 text-2xl font-black ${tone === "warning" ? "text-amber-700" : tone === "ok" ? "text-emerald-700" : "text-slate-950"}`}>
        {value}
      </p>
    </div>
  );
}

function ViewButton({
  active,
  icon: Icon,
  label,
  onClick
}: {
  readonly active: boolean;
  readonly icon: LucideIcon;
  readonly label: string;
  readonly onClick: () => void;
}) {
  return (
    <button
      type="button"
      className={`inline-flex min-h-11 flex-1 items-center justify-center gap-2 rounded-md px-3 text-sm font-black ${active
        ? "bg-slate-950 text-white"
        : "text-slate-700 hover:bg-slate-100"}`}
      onClick={onClick}
    >
      <Icon aria-hidden="true" className="size-4" />
      <span>{label}</span>
    </button>
  );
}

function Quantity({ label, value }: { readonly label: string; readonly value: number }) {
  return (
    <div className="rounded-md bg-slate-100 px-3 py-2">
      <p className="text-xs font-black uppercase text-slate-500">{label}</p>
      <p className="mt-1 font-black text-slate-950">{formatQuantity(value)}</p>
    </div>
  );
}

function StateMessage({
  icon: Icon,
  title,
  spin = false
}: {
  readonly icon: LucideIcon;
  readonly title: string;
  readonly spin?: boolean;
}) {
  return (
    <section className="grid min-h-64 place-items-center rounded-lg border border-slate-200 bg-white p-8 text-center">
      <div>
        <Icon aria-hidden="true" className={`mx-auto size-8 text-slate-400 ${spin ? "animate-spin" : ""}`} />
        <h2 className="mt-3 text-lg font-black text-slate-950">{title}</h2>
      </div>
    </section>
  );
}

function formatQuantity(value: number) {
  return new Intl.NumberFormat("pl-PL", {
    maximumFractionDigits: 3
  }).format(value);
}
