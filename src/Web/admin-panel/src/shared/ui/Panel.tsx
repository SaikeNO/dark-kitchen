import type { ReactNode } from "react";

export function Panel({ title, children }: { readonly title?: string; readonly children: ReactNode }) {
  return (
    <section className="rounded-lg border border-zinc-200 bg-white p-4 shadow-sm dark:border-zinc-800 dark:bg-zinc-950">
      {title !== undefined && <h3 className="mb-4 text-base font-bold text-zinc-950 dark:text-zinc-50">{title}</h3>}
      {children}
    </section>
  );
}
