import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Building2, CircleCheck, Pencil, Power, Save, X } from "lucide-react";
import { type FormEvent, useState } from "react";
import { toast } from "sonner";
import { errorMessage } from "../../api/http";
import { queryKeys } from "../../api/queryKeys";
import { Button } from "../../shared/ui/Button";
import { EmptyState } from "../../shared/ui/EmptyState";
import { Checkbox, Field, Textarea, TextInput } from "../../shared/ui/Fields";
import { PageHeader } from "../../shared/ui/PageHeader";
import { Panel } from "../../shared/ui/Panel";
import { StatusBadge } from "../../shared/ui/StatusBadge";
import { deactivateBrand, saveBrand } from "./brandsApi";
import type { Brand } from "./brandTypes";

const emptyForm = { name: "", description: "", logoUrl: "", isActive: true };

export function BrandsPage({ brands, canWrite }: { readonly brands: Brand[]; readonly canWrite: boolean }) {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: (action: () => Promise<unknown>) => action(),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.brands });
      toast.success("Zapisano markę.");
      clearForm();
    },
    onError: error => toast.error(errorMessage(error))
  });

  function clearForm() {
    setForm(emptyForm);
    setEditingId(null);
  }

  function edit(brand: Brand) {
    setEditingId(brand.id);
    setForm({
      name: brand.name,
      description: brand.description ?? "",
      logoUrl: brand.logoUrl ?? "",
      isActive: brand.isActive
    });
  }

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    mutation.mutate(() => saveBrand(editingId, form));
  }

  function changeStatus(brand: Brand, isActive: boolean) {
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    mutation.mutate(() => isActive
      ? saveBrand(brand.id, {
        name: brand.name,
        description: brand.description ?? "",
        logoUrl: brand.logoUrl ?? "",
        isActive: true
      })
      : deactivateBrand(brand.id));
  }

  return (
    <div className="grid gap-4">
      <PageHeader title="Marki" description="Dodawaj i edytuj marki widoczne w katalogu." />
      <div className="grid gap-4 lg:grid-cols-[360px_1fr]">
        <Panel title={editingId === null ? "Nowa marka" : "Edycja marki"}>
          <form className="grid gap-4" onSubmit={submit}>
            <Field label="Nazwa">
              <TextInput value={form.name} disabled={!canWrite} onChange={event => setForm({ ...form, name: event.currentTarget.value })} />
            </Field>
            <Field label="Opis">
              <Textarea value={form.description} disabled={!canWrite} onChange={event => setForm({ ...form, description: event.currentTarget.value })} />
            </Field>
            <Field label="Logo URL">
              <TextInput value={form.logoUrl} disabled={!canWrite} onChange={event => setForm({ ...form, logoUrl: event.currentTarget.value })} />
            </Field>
            <Checkbox label="Aktywna" checked={form.isActive} disabled={!canWrite} onChange={event => setForm({ ...form, isActive: event.currentTarget.checked })} />
            <div className="grid gap-2 sm:grid-cols-2">
              <Button type="submit" variant="primary" icon={Save} disabled={!canWrite || mutation.isPending}>Zapisz</Button>
              <Button type="button" icon={X} onClick={clearForm}>Wyczyść</Button>
            </div>
          </form>
        </Panel>

        <Panel title="Lista marek">
          {brands.length === 0 ? (
            <EmptyState icon={Building2} title="Brak marek" description="Dodaj pierwszą markę, aby zacząć budowę menu." />
          ) : (
            <div className="grid gap-3">
              {brands.map(brand => (
                <article key={brand.id} className="grid gap-3 rounded-lg border border-zinc-200 p-3 dark:border-zinc-800 sm:grid-cols-[1fr_auto] sm:items-center">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <h3 className="truncate font-bold text-zinc-950 dark:text-zinc-50">{brand.name}</h3>
                      <StatusBadge isActive={brand.isActive} />
                    </div>
                    <p className="mt-1 line-clamp-2 text-sm text-zinc-600 dark:text-zinc-400">{brand.description ?? "Brak opisu"}</p>
                  </div>
                  <div className="grid gap-2 sm:flex">
                    <Button icon={Pencil} onClick={() => edit(brand)}>Edytuj</Button>
                    {brand.isActive ? (
                      <Button
                        icon={Power}
                        variant="danger"
                        disabled={!canWrite || mutation.isPending}
                        onClick={() => changeStatus(brand, false)}
                      >
                        Dezaktywuj
                      </Button>
                    ) : (
                      <Button
                        icon={CircleCheck}
                        disabled={!canWrite || mutation.isPending}
                        onClick={() => changeStatus(brand, true)}
                      >
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
    </div>
  );
}
