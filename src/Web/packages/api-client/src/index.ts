export interface ApiClient {
  readonly baseUrl: string;
  readonly isConfigured: boolean;
  buildUrl(path: string): string;
}

export function createApiClient(baseUrl: string | undefined): ApiClient {
  let normalizedBaseUrl = baseUrl ?? "";

  // Remove trailing slashes
  while (normalizedBaseUrl.endsWith("/")) {
    normalizedBaseUrl = normalizedBaseUrl.slice(0, -1);
  }

  return {
    baseUrl: normalizedBaseUrl,
    isConfigured: normalizedBaseUrl.length > 0,
    buildUrl(path) {
      if (normalizedBaseUrl.length === 0) {
        return path;
      }

      // Remove leading slashes
      let cleanPath = path;
      while (cleanPath.startsWith("/")) {
        cleanPath = cleanPath.slice(1);
      }

      return `${normalizedBaseUrl}/${cleanPath}`;
    }
  };
}
