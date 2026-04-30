export interface ClientConfig {
  readonly apiBaseUrl: string | undefined;
}

function readViteEnvironmentVariable(name: string): string | undefined {
  const environment = import.meta.env as unknown as Record<string, unknown>;
  const value = environment[name];

  return typeof value === "string" && value.length > 0 ? value : undefined;
}

export const clientConfig = {
  apiBaseUrl: readViteEnvironmentVariable("VITE_API_BASE_URL")
} satisfies ClientConfig;
