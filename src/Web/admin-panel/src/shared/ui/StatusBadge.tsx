export function StatusBadge({ isActive }: { readonly isActive: boolean }) {
  return (
    <span className={isActive
      ? "inline-flex rounded-full bg-emerald-100 px-2 py-1 text-xs font-bold text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300"
      : "inline-flex rounded-full bg-zinc-100 px-2 py-1 text-xs font-bold text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300"}
    >
      {isActive ? "Aktywne" : "Nieaktywne"}
    </span>
  );
}
