import { ApiRequestError, getJson, postJson } from "../../api/http";
import { isSession } from "./authTypes";

export async function getCurrentSession(signal?: AbortSignal) {
  try {
    const session = await getJson<unknown>("/api/admin/auth/me", signal);
    return isSession(session) ? session : null;
  } catch (error) {
    if (error instanceof ApiRequestError) {
      return null;
    }

    throw error;
  }
}

export async function loginAdmin(email: string, password: string) {
  const session = await postJson<unknown>("/api/admin/auth/login", { email, password });

  if (!isSession(session)) {
    throw new ApiRequestError("Niepoprawna odpowiedź logowania.", undefined);
  }

  return session;
}

export function logoutAdmin() {
  return postJson<void>("/api/admin/auth/logout");
}
