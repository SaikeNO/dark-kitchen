import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AlertTriangle,
  Boxes,
  Gauge,
  ListFilter,
  Loader2,
  Moon,
  PackageCheck,
  PackagePlus,
  RefreshCw,
  Search,
  SlidersHorizontal,
  Sun,
  type LucideIcon
} from "lucide-react";
import { type FormEvent, useEffect, useMemo, useState } from "react";
import { Toaster, toast } from "sonner";
import { apiConfigured, errorMessage } from "./api/http";
import { queryKeys } from "./api/queryKeys";
import { adjustInventoryItem, listInventoryItems, listShortages, recordDelivery } from "./inventoryApi";
import type { InventoryItem } from "./inventoryTypes";

type ViewMode = "all" | "shortages";
type ThemeMode = "dark" | "light";

const emptyItems: InventoryItem[] = [];

export function App() {
  const queryClient = useQueryClient();
  const [viewMode, setViewMode] = useState<ViewMode>("all");
  const [themeMode, setThemeMode] = useState<ThemeMode>(readInitialTheme);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [deliveryQuantity, setDeliveryQuantity] = useState("");
  const [adjustOnHand, setAdjustOnHand] = useState("");
  const [adjustMin, setAdjustMin] = useState("");
  const isDark = themeMode === "dark";

  useEffect(() => {
    document.documentElement.dataset.theme = themeMode;
    localStorage.setItem("dark-kitchen-inventory-theme", themeMode);
  }, [themeMode]);

  const query = useQuery({
    queryKey: viewMode === "shortages" ? queryKeys.inventoryShortages : queryKeys.inventoryItems,
    queryFn: ({ signal }) => viewMode === "shortages" ? listShortages(signal) : listInventoryItems(signal),
    staleTime: 15_000
  });

  const sourceItems = query.data ?? emptyItems;
  const sortedItems = useMemo(() => {
    const normalizedSearch = search.trim().toLocaleLowerCase("pl");
    return sourceItems
      .filter(item => normalizedSearch.length === 0 || item.name.toLocaleLowerCase("pl").includes(normalizedSearch))
      .sort((left, right) => {
        if (left.isBelowSafetyLevel !== right.isBelowSafetyLevel) {
          return left.isBelowSafetyLevel ? -1 : 1;
        }

        return left.name.localeCompare(right.name, "pl");
      });
  }, [sourceItems, search]);

  const selectedItem = sortedItems.find(item => item.ingredientId === selectedId) ?? sortedItems[0];
  const activeItemId = selectedItem?.ingredientId;
  const adjustmentOnHandValue = adjustOnHand.length > 0 ? adjustOnHand : selectedItem === undefined ? "" : String(selectedItem.onHandQuantity);
  const adjustmentMinValue = adjustMin.length > 0 ? adjustMin : selectedItem === undefined ? "" : String(selectedItem.minSafetyLevel);
  const shortageCount = sourceItems.filter(item => item.isBelowSafetyLevel).length;
  const reorderTotal = sourceItems.reduce((sum, item) => sum + item.reorderQuantity, 0);
  const availableTotal = sourceItems.reduce((sum, item) => sum + item.availableQuantity, 0);

  const mutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<InventoryItem>; readonly success: string }) => request.action(),
    onSuccess: async (item, request) => {
      setSelectedId(item.ingredientId);
      setDeliveryQuantity("");
      setAdjustOnHand(String(item.onHandQuantity));
      setAdjustMin(String(item.minSafetyLevel));
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.inventoryItems }),
        queryClient.invalidateQueries({ queryKey: queryKeys.inventoryShortages })
      ]);
      toast.success(request.success);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function refresh() {
    void Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.inventoryItems }),
      queryClient.invalidateQueries({ queryKey: queryKeys.inventoryShortages })
    ]);
  }

  function selectItem(item: InventoryItem) {
    setSelectedId(item.ingredientId);
    setAdjustOnHand(String(item.onHandQuantity));
    setAdjustMin(String(item.minSafetyLevel));
    setDeliveryQuantity("");
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
    <main className={`min-h-screen ${isDark ? "bg-[#020617] text-slate-100" : "bg-slate-100 text-slate-950"}`}>
      <div className="mx-auto min-h-screen max-w-md pb-24 md:max-w-6xl md:pb-8">
        <header className={`sticky top-0 z-30 border-b px-4 pb-4 pt-3 backdrop-blur-xl ${isDark ? "border-white/10 bg-[#020617]/92" : "border-slate-200 bg-slate-100/90"}`}>
          <div className="flex items-center justify-between gap-3">
            <div className="min-w-0">
              <p className={`text-xs font-black uppercase ${isDark ? "text-emerald-300" : "text-emerald-700"}`}>Dark Kitchen</p>
              <h1 className="truncate text-2xl font-black tracking-normal">Magazyn</h1>
            </div>
            <div className="flex shrink-0 items-center gap-2">
              <IconButton
                ariaLabel={isDark ? "Tryb jasny" : "Tryb ciemny"}
                icon={isDark ? Sun : Moon}
                isDark={isDark}
                onClick={() => setThemeMode(isDark ? "light" : "dark")}
              />
              <IconButton
                ariaLabel="Odswiez"
                icon={query.isFetching ? Loader2 : RefreshCw}
                isDark={isDark}
                spin={query.isFetching}
                onClick={refresh}
              />
            </div>
          </div>

          <div className={`mt-4 flex items-center gap-2 rounded-md border px-3 ${isDark ? "border-white/10 bg-white/7" : "border-slate-200 bg-white"}`}>
            <Search aria-hidden="true" className={`size-4 ${isDark ? "text-slate-400" : "text-slate-500"}`} />
            <input
              aria-label="Szukaj skladnika"
              className="min-h-11 min-w-0 flex-1 bg-transparent text-base outline-none placeholder:text-slate-500"
              placeholder="Szukaj skladnika"
              value={search}
              onChange={event => setSearch(event.currentTarget.value)}
            />
          </div>
        </header>

        <div className="grid gap-4 px-4 py-4 md:grid-cols-[minmax(0,1fr)_380px] md:items-start">
          <section className="grid gap-4">
            <div className="grid grid-cols-3 gap-2">
              <Metric isDark={isDark} icon={Boxes} label="Pozycje" value={String(sourceItems.length)} />
              <Metric isDark={isDark} icon={AlertTriangle} label="Braki" tone={shortageCount > 0 ? "warning" : "ok"} value={String(shortageCount)} />
              <Metric isDark={isDark} icon={Gauge} label="Dostepne" value={formatQuantity(availableTotal)} />
            </div>

            <section className={`grid grid-cols-2 gap-2 rounded-lg border p-1.5 ${isDark ? "border-white/10 bg-white/7" : "border-slate-200 bg-white"}`}>
              <ViewButton active={viewMode === "all"} icon={Boxes} isDark={isDark} label="Stany" onClick={() => setViewMode("all")} />
              <ViewButton active={viewMode === "shortages"} icon={ListFilter} isDark={isDark} label="Braki" onClick={() => setViewMode("shortages")} />
            </section>

            {query.isPending ? (
              <StateMessage icon={Loader2} isDark={isDark} title="Ladowanie stanow" spin />
            ) : query.isError ? (
              <StateMessage icon={AlertTriangle} isDark={isDark} title="Nie udalo sie pobrac danych" />
            ) : sortedItems.length === 0 ? (
              <StateMessage icon={PackageCheck} isDark={isDark} title={viewMode === "shortages" ? "Brak brakow" : "Brak pozycji"} />
            ) : (
              <div className="grid gap-3">
                {sortedItems.map(item => (
                  <InventoryRow
                    active={item.ingredientId === activeItemId}
                    isDark={isDark}
                    item={item}
                    key={item.ingredientId}
                    onClick={() => selectItem(item)}
                  />
                ))}
              </div>
            )}
          </section>

          {selectedItem !== undefined && (
            <InventoryActions
              adjustMin={adjustmentMinValue}
              adjustOnHand={adjustmentOnHandValue}
              deliveryQuantity={deliveryQuantity}
              isDark={isDark}
              item={selectedItem}
              mutationPending={mutation.isPending}
              reorderTotal={reorderTotal}
              setAdjustMin={setAdjustMin}
              setAdjustOnHand={setAdjustOnHand}
              setDeliveryQuantity={setDeliveryQuantity}
              submitAdjustment={submitAdjustment}
              submitDelivery={submitDelivery}
            />
          )}
        </div>

        <footer className={`px-4 pb-4 text-xs font-semibold ${isDark ? "text-slate-500" : "text-slate-500"}`}>
          API: {apiConfigured ? "configured" : "relative URL"}
        </footer>
      </div>
      <Toaster richColors position="bottom-center" theme={themeMode} />
    </main>
  );
}

function InventoryActions({
  adjustMin,
  adjustOnHand,
  deliveryQuantity,
  isDark,
  item,
  mutationPending,
  reorderTotal,
  setAdjustMin,
  setAdjustOnHand,
  setDeliveryQuantity,
  submitAdjustment,
  submitDelivery
}: {
  readonly adjustMin: string;
  readonly adjustOnHand: string;
  readonly deliveryQuantity: string;
  readonly isDark: boolean;
  readonly item: InventoryItem;
  readonly mutationPending: boolean;
  readonly reorderTotal: number;
  readonly setAdjustMin: (value: string) => void;
  readonly setAdjustOnHand: (value: string) => void;
  readonly setDeliveryQuantity: (value: string) => void;
  readonly submitAdjustment: (event: FormEvent<HTMLFormElement>) => void;
  readonly submitDelivery: (event: FormEvent<HTMLFormElement>) => void;
}) {
  return (
    <aside className={`fixed inset-x-0 bottom-0 z-40 border-t px-4 pb-4 pt-3 shadow-2xl md:sticky md:top-24 md:inset-x-auto md:rounded-lg md:border ${
      isDark ? "border-white/10 bg-[#07111f]/96 shadow-black/50" : "border-slate-200 bg-white/96 shadow-slate-300/60"
    } backdrop-blur-xl`}>
      <div className="mx-auto max-w-md md:max-w-none">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <h2 className="truncate text-lg font-black">{item.name}</h2>
            <p className={`mt-0.5 text-sm font-bold ${isDark ? "text-slate-400" : "text-slate-500"}`}>
              {formatQuantity(item.availableQuantity)} {item.unit} dostepne
            </p>
          </div>
          {item.isBelowSafetyLevel ? (
            <span className={`inline-flex shrink-0 items-center gap-1 rounded-md px-2 py-1 text-xs font-black ${
              isDark ? "bg-amber-400/15 text-amber-200" : "bg-amber-100 text-amber-800"
            }`}>
              <AlertTriangle aria-hidden="true" className="size-3" />
              Zamow
            </span>
          ) : (
            <span className={`inline-flex shrink-0 items-center gap-1 rounded-md px-2 py-1 text-xs font-black ${
              isDark ? "bg-emerald-400/15 text-emerald-200" : "bg-emerald-100 text-emerald-800"
            }`}>
              <PackageCheck aria-hidden="true" className="size-3" />
              OK
            </span>
          )}
        </div>

        <div className="mt-3 grid grid-cols-4 gap-2 text-center">
          <MiniStat isDark={isDark} label="Fizyczne" value={item.onHandQuantity} />
          <MiniStat isDark={isDark} label="Rez." value={item.reservedQuantity} />
          <MiniStat isDark={isDark} label="Min." value={item.minSafetyLevel} />
          <MiniStat isDark={isDark} label="Kupic" tone={item.isBelowSafetyLevel ? "warning" : undefined} value={item.reorderQuantity} />
        </div>

        <form className="mt-4 grid grid-cols-[minmax(0,1fr)_auto] gap-2" onSubmit={submitDelivery}>
          <NumberField
            isDark={isDark}
            label={`Dostawa (${item.unit})`}
            min="0"
            onChange={setDeliveryQuantity}
            value={deliveryQuantity}
          />
          <button
            type="submit"
            disabled={mutationPending}
            className="mt-6 inline-flex min-h-11 items-center justify-center gap-2 rounded-md bg-emerald-500 px-4 text-sm font-black text-slate-950 transition-colors hover:bg-emerald-400 disabled:opacity-50"
          >
            <PackagePlus aria-hidden="true" className="size-4" />
            <span>Dodaj</span>
          </button>
        </form>

        <form className="mt-3 grid gap-2" onSubmit={submitAdjustment}>
          <div className="grid grid-cols-2 gap-2">
            <NumberField isDark={isDark} label="Stan" min={String(item.reservedQuantity)} onChange={setAdjustOnHand} value={adjustOnHand} />
            <NumberField isDark={isDark} label="Minimum" min="0" onChange={setAdjustMin} value={adjustMin} />
          </div>
          <button
            type="submit"
            disabled={mutationPending}
            className={`inline-flex min-h-11 items-center justify-center gap-2 rounded-md border px-4 text-sm font-black transition-colors disabled:opacity-50 ${
              isDark ? "border-white/10 bg-white/8 text-slate-100 hover:bg-white/12" : "border-slate-200 bg-slate-950 text-white hover:bg-slate-800"
            }`}
          >
            <SlidersHorizontal aria-hidden="true" className="size-4" />
            <span>Zapisz korekte</span>
          </button>
        </form>

        <p className={`mt-3 text-xs font-bold ${isDark ? "text-slate-500" : "text-slate-500"}`}>
          Lacznie do zamowienia: {formatQuantity(reorderTotal)} jednostek.
        </p>
      </div>
    </aside>
  );
}

function InventoryRow({
  active,
  isDark,
  item,
  onClick
}: {
  readonly active: boolean;
  readonly isDark: boolean;
  readonly item: InventoryItem;
  readonly onClick: () => void;
}) {
  const percent = item.minSafetyLevel > 0
    ? Math.min(100, Math.max(0, (item.availableQuantity / item.minSafetyLevel) * 100))
    : 100;

  return (
    <button
      type="button"
      className={`rounded-lg border p-3 text-left transition-colors ${
        active
          ? isDark ? "border-emerald-400 bg-emerald-400/10" : "border-emerald-500 bg-emerald-50"
          : isDark ? "border-white/10 bg-white/6 hover:bg-white/10" : "border-slate-200 bg-white hover:border-slate-300"
      }`}
      onClick={onClick}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h2 className="truncate text-base font-black">{item.name}</h2>
          <p className={`mt-0.5 text-sm font-bold ${isDark ? "text-slate-400" : "text-slate-500"}`}>
            {item.unit}
          </p>
        </div>
        <p className={`shrink-0 text-right text-lg font-black ${item.isBelowSafetyLevel ? "text-amber-500" : isDark ? "text-emerald-300" : "text-emerald-700"}`}>
          {formatQuantity(item.availableQuantity)}
        </p>
      </div>
      <div className={`mt-3 h-2 overflow-hidden rounded-full ${isDark ? "bg-white/10" : "bg-slate-100"}`}>
        <div
          className={`h-full rounded-full ${item.isBelowSafetyLevel ? "bg-amber-400" : "bg-emerald-500"}`}
          style={{ width: `${percent}%` }}
        />
      </div>
      <div className={`mt-2 flex items-center justify-between text-xs font-bold ${isDark ? "text-slate-400" : "text-slate-500"}`}>
        <span>Fiz. {formatQuantity(item.onHandQuantity)}</span>
        <span>Rez. {formatQuantity(item.reservedQuantity)}</span>
        <span>Min. {formatQuantity(item.minSafetyLevel)}</span>
      </div>
    </button>
  );
}

function Metric({
  icon: Icon,
  isDark,
  label,
  tone,
  value
}: {
  readonly icon: LucideIcon;
  readonly isDark: boolean;
  readonly label: string;
  readonly tone?: "ok" | "warning";
  readonly value: string;
}) {
  return (
    <div className={`min-h-24 rounded-lg border p-3 ${isDark ? "border-white/10 bg-white/7" : "border-slate-200 bg-white"}`}>
      <Icon aria-hidden="true" className={`size-4 ${tone === "warning" ? "text-amber-500" : tone === "ok" ? "text-emerald-500" : isDark ? "text-slate-400" : "text-slate-500"}`} />
      <p className={`mt-3 text-xs font-black uppercase ${isDark ? "text-slate-400" : "text-slate-500"}`}>{label}</p>
      <p className={`mt-1 truncate text-xl font-black ${tone === "warning" ? "text-amber-500" : tone === "ok" ? "text-emerald-500" : ""}`}>
        {value}
      </p>
    </div>
  );
}

function MiniStat({
  isDark,
  label,
  tone,
  value
}: {
  readonly isDark: boolean;
  readonly label: string;
  readonly tone?: "warning";
  readonly value: number;
}) {
  return (
    <div className={`rounded-md px-2 py-2 ${isDark ? "bg-white/7" : "bg-slate-100"}`}>
      <p className={`text-xs font-black uppercase ${isDark ? "text-slate-500" : "text-slate-500"}`}>{label}</p>
      <p className={`mt-1 truncate text-sm font-black ${tone === "warning" ? "text-amber-500" : ""}`}>{formatQuantity(value)}</p>
    </div>
  );
}

function NumberField({
  isDark,
  label,
  min,
  onChange,
  value
}: {
  readonly isDark: boolean;
  readonly label: string;
  readonly min: string;
  readonly onChange: (value: string) => void;
  readonly value: string;
}) {
  return (
    <label className={`grid gap-1 text-xs font-black uppercase ${isDark ? "text-slate-400" : "text-slate-600"}`}>
      {label}
      <input
        className={`min-h-11 min-w-0 rounded-md border px-3 text-base font-bold outline-none ${
          isDark ? "border-white/10 bg-[#020617] text-slate-100" : "border-slate-200 bg-white text-slate-950"
        }`}
        inputMode="decimal"
        min={min}
        step="0.001"
        type="number"
        value={value}
        onChange={event => onChange(event.currentTarget.value)}
      />
    </label>
  );
}

function IconButton({
  ariaLabel,
  icon: Icon,
  isDark,
  onClick,
  spin = false
}: {
  readonly ariaLabel: string;
  readonly icon: LucideIcon;
  readonly isDark: boolean;
  readonly onClick: () => void;
  readonly spin?: boolean;
}) {
  return (
    <button
      type="button"
      aria-label={ariaLabel}
      className={`grid size-11 place-items-center rounded-md border transition-colors ${
        isDark ? "border-white/10 bg-white/7 text-slate-100 hover:bg-white/12" : "border-slate-200 bg-white text-slate-900 hover:bg-slate-50"
      }`}
      onClick={onClick}
    >
      <Icon aria-hidden="true" className={`size-5 ${spin ? "animate-spin" : ""}`} />
    </button>
  );
}

function ViewButton({
  active,
  icon: Icon,
  isDark,
  label,
  onClick
}: {
  readonly active: boolean;
  readonly icon: LucideIcon;
  readonly isDark: boolean;
  readonly label: string;
  readonly onClick: () => void;
}) {
  return (
    <button
      type="button"
      className={`inline-flex min-h-11 items-center justify-center gap-2 rounded-md px-3 text-sm font-black transition-colors ${
        active
          ? "bg-emerald-500 text-slate-950"
          : isDark ? "text-slate-300 hover:bg-white/8" : "text-slate-700 hover:bg-slate-100"
      }`}
      onClick={onClick}
    >
      <Icon aria-hidden="true" className="size-4" />
      <span>{label}</span>
    </button>
  );
}

function StateMessage({
  icon: Icon,
  isDark,
  title,
  spin = false
}: {
  readonly icon: LucideIcon;
  readonly isDark: boolean;
  readonly title: string;
  readonly spin?: boolean;
}) {
  return (
    <section className={`grid min-h-64 place-items-center rounded-lg border p-8 text-center ${
      isDark ? "border-white/10 bg-white/6" : "border-slate-200 bg-white"
    }`}>
      <div>
        <Icon aria-hidden="true" className={`mx-auto size-8 ${isDark ? "text-slate-500" : "text-slate-400"} ${spin ? "animate-spin" : ""}`} />
        <h2 className="mt-3 text-lg font-black">{title}</h2>
      </div>
    </section>
  );
}

function formatQuantity(value: number) {
  return new Intl.NumberFormat("pl-PL", {
    maximumFractionDigits: 3
  }).format(value);
}

function readInitialTheme(): ThemeMode {
  if (typeof window === "undefined") {
    return "dark";
  }

  const saved = localStorage.getItem("dark-kitchen-inventory-theme");
  if (saved === "dark" || saved === "light") {
    return saved;
  }

  return window.matchMedia("(prefers-color-scheme: light)").matches ? "light" : "dark";
}
