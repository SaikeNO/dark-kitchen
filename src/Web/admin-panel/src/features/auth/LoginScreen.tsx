import { useMutation, useQueryClient } from "@tanstack/react-query";
import { KeyRound, Moon, ShieldCheck, Sun, UserRound } from "lucide-react";
import { type FormEvent, useState } from "react";
import { toast } from "sonner";
import { queryKeys } from "../../api/queryKeys";
import { Button } from "../../shared/ui/Button";
import { Field, TextInput } from "../../shared/ui/Fields";
import { errorMessage } from "../../api/http";
import { appMetadata } from "../../appMetadata";
import { loginAdmin } from "./authApi";
import type { Session } from "./authTypes";

const demoPassword = "Demo123!";

export function LoginScreen({
  darkMode,
  onToggleTheme
}: {
  readonly darkMode: boolean;
  readonly onToggleTheme: () => void;
}) {
  const queryClient = useQueryClient();
  const [email, setEmail] = useState("manager@darkkitchen.local");
  const [password, setPassword] = useState(demoPassword);

  const loginMutation = useMutation({
    mutationFn: (request: { readonly email: string; readonly password: string }) => loginAdmin(request.email, request.password),
    onSuccess: session => {
      queryClient.setQueryData<Session | null>(queryKeys.session, session);
      toast.success("Zalogowano.");
    },
    onError: error => toast.error(errorMessage(error, "Logowanie nie powiodło się."))
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    loginMutation.mutate({ email, password });
  }

  function pickDemo(nextEmail: string) {
    setEmail(nextEmail);
    setPassword(demoPassword);
  }

  return (
    <main className="min-h-screen bg-zinc-50 p-4 text-zinc-950 dark:bg-zinc-950 dark:text-zinc-50">
      <div className="mx-auto grid min-h-[calc(100vh-2rem)] max-w-5xl content-center gap-6 lg:grid-cols-[1fr_420px]">
        <section className="flex flex-col justify-end rounded-lg border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-800 dark:bg-zinc-900">
          <div className="mb-8 flex items-center justify-between">
            <p className="text-sm font-black uppercase tracking-normal text-emerald-700 dark:text-emerald-400">Dark Kitchen</p>
            <Button variant="ghost" icon={darkMode ? Sun : Moon} onClick={onToggleTheme}>
              {darkMode ? "Light" : "Dark"}
            </Button>
          </div>
          <h1 className="max-w-xl text-4xl font-black tracking-tight sm:text-5xl">{appMetadata.name}</h1>
          <p className="mt-3 max-w-xl text-base text-zinc-600 dark:text-zinc-400">{appMetadata.description}</p>
        </section>

        <section className="rounded-lg border border-zinc-200 bg-white p-5 shadow-sm dark:border-zinc-800 dark:bg-zinc-900" aria-labelledby="login-title">
          <div className="mb-5 flex items-center gap-2 text-sm font-bold text-emerald-700 dark:text-emerald-400">
            <KeyRound aria-hidden="true" className="size-4" />
            Dostęp administracyjny
          </div>
          <h2 id="login-title" className="text-2xl font-bold">Sign in</h2>
          <form className="mt-5 grid gap-4" onSubmit={handleSubmit}>
            <Field label="Email">
              <TextInput
                autoComplete="username"
                type="email"
                value={email}
                onChange={event => setEmail(event.currentTarget.value)}
              />
            </Field>
            <Field label="Password">
              <TextInput
                autoComplete="current-password"
                type="password"
                value={password}
                onChange={event => setPassword(event.currentTarget.value)}
              />
            </Field>
            <div className="grid gap-2 sm:grid-cols-2" aria-label="Demo accounts">
              <Button type="button" icon={UserRound} onClick={() => pickDemo("manager@darkkitchen.local")}>
                Manager demo
              </Button>
              <Button type="button" icon={ShieldCheck} onClick={() => pickDemo("operator@darkkitchen.local")}>
                Operator demo
              </Button>
            </div>
            <Button type="submit" icon={KeyRound} variant="primary" disabled={loginMutation.isPending}>
              Sign in
            </Button>
          </form>
        </section>
      </div>
    </main>
  );
}
