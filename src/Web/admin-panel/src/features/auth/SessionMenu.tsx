import { useMutation, useQueryClient } from "@tanstack/react-query";
import { LogOut, Moon, Sun } from "lucide-react";
import { toast } from "sonner";
import { errorMessage } from "../../api/http";
import { queryKeys } from "../../api/queryKeys";
import { Button } from "../../shared/ui/Button";
import { logoutAdmin } from "./authApi";
import type { Session } from "./authTypes";

export function SessionMenu({
  session,
  darkMode,
  onToggleTheme
}: {
  readonly session: Session;
  readonly darkMode: boolean;
  readonly onToggleTheme: () => void;
}) {
  const queryClient = useQueryClient();
  const isManager = session.roles.includes("Manager");
  const logoutMutation = useMutation({
    mutationFn: logoutAdmin,
    onSettled: () => {
      queryClient.setQueryData<Session | null>(queryKeys.session, null);
      queryClient.removeQueries({ queryKey: ["catalog"] });
    },
    onError: error => toast.error(errorMessage(error, "Wylogowanie nie powiodło się."))
  });

  return (
    <div className="flex flex-wrap items-center gap-2">
      <span className="max-w-[13rem] truncate text-sm text-zinc-600 dark:text-zinc-400">{session.email}</span>
      <span className="rounded-full bg-zinc-100 px-2 py-1 text-xs font-bold text-zinc-700 dark:bg-zinc-800 dark:text-zinc-200">
        {isManager ? "Manager" : "Operator"}
      </span>
      <Button variant="ghost" icon={darkMode ? Sun : Moon} onClick={onToggleTheme}>
        {darkMode ? "Light" : "Dark"}
      </Button>
      <Button icon={LogOut} onClick={() => logoutMutation.mutate()} disabled={logoutMutation.isPending}>
        Logout
      </Button>
    </div>
  );
}
