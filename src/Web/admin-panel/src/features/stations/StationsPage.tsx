import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ChefHat, CircleCheck, Pencil, Power, Save, X } from "lucide-react";
import { type FormEvent, useState } from "react";
import { toast } from "sonner";
import { errorMessage } from "../../api/http";
import { queryKeys } from "../../api/queryKeys";
import { Button } from "../../shared/ui/Button";
import { EmptyState } from "../../shared/ui/EmptyState";
import { Checkbox, Field, TextInput } from "../../shared/ui/Fields";
import { PageHeader } from "../../shared/ui/PageHeader";
import { Panel } from "../../shared/ui/Panel";
import { StatusBadge } from "../../shared/ui/StatusBadge";
import { deactivateStation, saveStation } from "./stationsApi";
import type { Station } from "./stationTypes";

const emptyForm = { code: "", name: "", displayColor: "#22c55e", isActive: true };

export function StationsPage({ stations, canWrite }: { readonly stations: Station[]; readonly canWrite: boolean }) {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<unknown>; readonly success: string }) => request.action(),
    onSuccess: async (_data, request) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.stations });
      toast.success(request.success);
    },
    onError: error => toast.error(errorMessage(error))
  });

  function clearForm() {
    setForm(emptyForm);
    setEditingId(null);
  }

  function edit(station: Station) {
    setEditingId(station.id);
    setForm({
      code: station.code,
      name: station.name,
      displayColor: station.displayColor,
      isActive: station.isActive
    });
  }

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    mutation.mutate({
      action: () => saveStation(editingId, form),
      success: "Zapisano stację."
    }, {
      onSuccess: clearForm
    });
  }

  function changeStatus(station: Station, isActive: boolean) {
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    mutation.mutate({
      action: () => isActive
        ? saveStation(station.id, {
          code: station.code,
          name: station.name,
          displayColor: station.displayColor,
          isActive: true
        })
        : deactivateStation(station.id),
      success: isActive ? "Aktywowano stację." : "Dezaktywowano stację."
    });
  }

  return (
    <div className="grid gap-4">
      <PageHeader title="Stacje" description="Konfiguruj stanowiska kuchenne używane przez KDS i routing produktów." />
      <div className="grid gap-4 lg:grid-cols-[360px_1fr]">
        <Panel title={editingId === null ? "Nowa stacja" : "Edycja stacji"}>
          <form className="grid gap-4" onSubmit={submit}>
            <Field label="Kod">
              <TextInput value={form.code} disabled={!canWrite} onChange={event => setForm({ ...form, code: event.currentTarget.value })} />
            </Field>
            <Field label="Nazwa">
              <TextInput value={form.name} disabled={!canWrite} onChange={event => setForm({ ...form, name: event.currentTarget.value })} />
            </Field>
            <Field label="Kolor">
              <TextInput type="color" value={form.displayColor} disabled={!canWrite} onChange={event => setForm({ ...form, displayColor: event.currentTarget.value })} className="h-11 p-1" />
            </Field>
            <Checkbox label="Aktywna" checked={form.isActive} disabled={!canWrite} onChange={event => setForm({ ...form, isActive: event.currentTarget.checked })} />
            <div className="grid gap-2 sm:grid-cols-2">
              <Button type="submit" variant="primary" icon={Save} disabled={!canWrite || mutation.isPending}>Zapisz</Button>
              <Button type="button" icon={X} onClick={clearForm}>Wyczyść</Button>
            </div>
          </form>
        </Panel>

        <Panel title="Lista stacji">
          {stations.length === 0 ? (
            <EmptyState icon={ChefHat} title="Brak stacji" description="Dodaj stanowiska, żeby przypisywać produkty do przygotowania." />
          ) : (
            <div className="grid gap-3 md:grid-cols-2">
              {stations.map(station => (
                <article key={station.id} className="rounded-lg border border-zinc-200 p-3 dark:border-zinc-800">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="size-3 rounded-full" style={{ backgroundColor: station.displayColor }} />
                    <h3 className="font-bold text-zinc-950 dark:text-zinc-50">{station.code}</h3>
                    <StatusBadge isActive={station.isActive} />
                  </div>
                  <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">{station.name}</p>
                  <div className="mt-3 flex flex-wrap gap-2">
                    <Button icon={Pencil} onClick={() => edit(station)}>Edytuj</Button>
                    {station.isActive ? (
                      <Button icon={Power} variant="danger" disabled={!canWrite || mutation.isPending} onClick={() => changeStatus(station, false)}>
                        Dezaktywuj
                      </Button>
                    ) : (
                      <Button icon={CircleCheck} disabled={!canWrite || mutation.isPending} onClick={() => changeStatus(station, true)}>
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
