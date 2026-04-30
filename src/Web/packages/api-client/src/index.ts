export interface ApiClient {
  readonly baseUrl: string;
  readonly isConfigured: boolean;
  buildUrl(path: string): string;
}

export function createApiClient(baseUrl: string | undefined): ApiClient {
  const normalizedBaseUrl = baseUrl?.replace(/\/+$/, "") ?? "";

  return {
    baseUrl: normalizedBaseUrl,
    isConfigured: normalizedBaseUrl.length > 0,
    buildUrl(path) {
      if (normalizedBaseUrl.length === 0) {
        return path;
      }

      return `${normalizedBaseUrl}/${path.replace(/^\/+/, "")}`;
    }
  };
}
