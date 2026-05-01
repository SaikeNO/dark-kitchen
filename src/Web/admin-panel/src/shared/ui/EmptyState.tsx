import type { LucideIcon } from "lucide-react";

export function EmptyState({
  icon: Icon,
  title,
  description
}: {
  readonly icon: LucideIcon;
  readonly title: string;
  readonly description?: string;
}) {
  return (
    <div className="grid place-items-center gap-2 rounded-lg border border-dashed border-zinc-300 bg-white p-8 text-center dark:border-zinc-800 dark:bg-zinc-950">
      <Icon aria-hidden="true" className="size-8 text-zinc-400" />
      <strong className="text-zinc-900 dark:text-zinc-50">{title}</strong>
      {description !== undefined && <p className="max-w-sm text-sm text-zinc-500 dark:text-zinc-400">{description}</p>}
    </div>
  );
}
