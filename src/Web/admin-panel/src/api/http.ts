import { createApiClient } from "@dark-kitchen/api-client";
import { clientConfig } from "@dark-kitchen/config";
import axios from "axios";

const apiClient = createApiClient(clientConfig.apiBaseUrl);

export const apiConfigured = apiClient.isConfigured;

export class ApiRequestError extends Error {
  constructor(message: string, readonly status: number | undefined) {
    super(message);
    this.name = "ApiRequestError";
  }
}

interface ApiProblem {
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Record<string, string[]>;
}

export const http = axios.create({
  baseURL: apiClient.baseUrl.length > 0 ? apiClient.baseUrl : undefined,
  timeout: 10_000,
  withCredentials: true
});

http.interceptors.response.use(
  response => response,
  (error: unknown) => {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const problem = asApiProblem(error.response?.data);
      const fallback = status === undefined
        ? "Nie można połączyć się z API."
        : `Żądanie zakończone błędem ${status}.`;

      return Promise.reject(new ApiRequestError(problemMessage(problem, fallback), status));
    }

    return Promise.reject(error instanceof Error ? error : new Error("Nieznany błąd API."));
  }
);

export async function getJson<TResponse>(url: string, signal?: AbortSignal) {
  const response = await http.get<TResponse>(url, { signal });
  return response.data;
}

export async function postJson<TResponse>(url: string, data?: unknown) {
  const response = await http.post<TResponse>(url, data);
  return response.data;
}

export async function putJson<TResponse>(url: string, data: unknown) {
  const response = await http.put<TResponse>(url, data);
  return response.data;
}

export async function postForm<TResponse>(url: string, data: FormData) {
  const response = await http.post<TResponse>(url, data);
  return response.data;
}

export function errorMessage(error: unknown, fallback = "Operacja nie powiodła się.") {
  return error instanceof Error ? error.message : fallback;
}

function asApiProblem(data: unknown): ApiProblem | undefined {
  return typeof data === "object" && data !== null ? data : undefined;
}

function problemMessage(problem: ApiProblem | undefined, fallback: string) {
  if (problem?.errors !== undefined) {
    return Object.entries(problem.errors)
      .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
      .join(" ");
  }

  return problem?.detail ?? problem?.title ?? fallback;
}
