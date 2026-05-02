import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Building2, CircleCheck, ImageUp, Pencil, Power, Save, X } from "lucide-react";
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
import { deactivateBrand, saveBrand, uploadBrandLogo } from "./brandsApi";
import type { Brand } from "./brandTypes";

const emptyForm = {
  name: "",
  description: "",
  logoUrl: "",
  domainsText: "",
  heroTitle: "",
  heroSubtitle: "",
  primaryColor: "#dc2626",
  accentColor: "#ca8a04",
  backgroundColor: "#fef2f2",
  textColor: "#450a0a",
  isActive: true
};

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
      domainsText: brand.domains.join("\n"),
      heroTitle: brand.heroTitle ?? "",
      heroSubtitle: brand.heroSubtitle ?? "",
      primaryColor: brand.primaryColor,
      accentColor: brand.accentColor,
      backgroundColor: brand.backgroundColor,
      textColor: brand.textColor,
      isActive: brand.isActive
    });
  }

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!canWrite) {
      toast.error("Operator ma tylko odczyt.");
      return;
    }

    mutation.mutate(() => saveBrand(editingId, toPayload(form)));
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
        domains: brand.domains,
        heroTitle: brand.heroTitle ?? "",
        heroSubtitle: brand.heroSubtitle ?? "",
        primaryColor: brand.primaryColor,
        accentColor: brand.accentColor,
        backgroundColor: brand.backgroundColor,
        textColor: brand.textColor,
        isActive: true
      })
      : deactivateBrand(brand.id));
  }

  async function uploadLogo(file: File | undefined) {
    if (file === undefined) {
      return;
    }

    try {
      const uploaded = await uploadBrandLogo(file);
      setForm(current => ({ ...current, logoUrl: uploaded.url }));
      toast.success("Wgrano logo.");
    } catch (error) {
      toast.error(errorMessage(error));
    }
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
            <Field label="Wgraj logo">
              <input
                type="file"
                accept="image/png,image/jpeg,image/webp"
                disabled={!canWrite}
                onChange={event => { void uploadLogo(event.currentTarget.files?.[0]); }}
              />
            </Field>
            {form.logoUrl.length > 0 && (
              <div className="flex items-center gap-3 rounded-lg border border-zinc-200 bg-zinc-50 p-3 dark:border-zinc-800 dark:bg-zinc-900">
                <div className="grid size-16 place-items-center overflow-hidden rounded-md bg-white dark:bg-zinc-950">
                  <img src={form.logoUrl} alt="" className="max-h-full max-w-full object-contain" />
                </div>
                <span className="min-w-0 truncate text-sm font-semibold text-zinc-600 dark:text-zinc-400">{form.logoUrl}</span>
              </div>
            )}
            <Field label="Domeny">
              <Textarea value={form.domainsText} disabled={!canWrite} placeholder="burgerghost.local" onChange={event => setForm({ ...form, domainsText: event.currentTarget.value })} />
            </Field>
            <Field label="Hero title">
              <TextInput value={form.heroTitle} disabled={!canWrite} onChange={event => setForm({ ...form, heroTitle: event.currentTarget.value })} />
            </Field>
            <Field label="Hero subtitle">
              <Textarea value={form.heroSubtitle} disabled={!canWrite} onChange={event => setForm({ ...form, heroSubtitle: event.currentTarget.value })} />
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <ColorField label="Primary" value={form.primaryColor} disabled={!canWrite} onChange={value => setForm({ ...form, primaryColor: value })} />
              <ColorField label="Accent" value={form.accentColor} disabled={!canWrite} onChange={value => setForm({ ...form, accentColor: value })} />
              <ColorField label="Background" value={form.backgroundColor} disabled={!canWrite} onChange={value => setForm({ ...form, backgroundColor: value })} />
              <ColorField label="Text" value={form.textColor} disabled={!canWrite} onChange={value => setForm({ ...form, textColor: value })} />
            </div>
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
                      {brand.logoUrl === null ? <ImageUp aria-hidden="true" className="size-4 text-zinc-400" /> : <img src={brand.logoUrl} alt="" className="size-7 rounded object-cover" />}
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

function ColorField({
  label,
  value,
  disabled,
  onChange
}: {
  readonly label: string;
  readonly value: string;
  readonly disabled: boolean;
  readonly onChange: (value: string) => void;
}) {
  return (
    <Field label={label}>
      <div className="grid grid-cols-[3rem_1fr] gap-2">
        <input type="color" value={value} disabled={disabled} onChange={event => onChange(event.currentTarget.value)} className="h-10 w-full rounded-md border border-zinc-200 bg-white p-1 dark:border-zinc-800 dark:bg-zinc-950" />
        <TextInput value={value} disabled={disabled} onChange={event => onChange(event.currentTarget.value)} />
      </div>
    </Field>
  );
}

function toPayload(form: typeof emptyForm) {
  return {
    name: form.name,
    description: form.description,
    logoUrl: form.logoUrl,
    domains: form.domainsText
      .split(/\r?\n|,/)
      .map(domain => domain.trim())
      .filter(domain => domain.length > 0),
    heroTitle: form.heroTitle,
    heroSubtitle: form.heroSubtitle,
    primaryColor: form.primaryColor,
    accentColor: form.accentColor,
    backgroundColor: form.backgroundColor,
    textColor: form.textColor,
    isActive: form.isActive
  };
}
