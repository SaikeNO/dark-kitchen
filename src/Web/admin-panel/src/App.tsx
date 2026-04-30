import { createApiClient } from "@dark-kitchen/api-client";
import { clientConfig } from "@dark-kitchen/config";
import { AppShell } from "@dark-kitchen/ui";
import "@dark-kitchen/ui/app-shell.css";
import { appMetadata } from "./appMetadata";
import "./styles.css";

const apiClient = createApiClient(clientConfig.apiBaseUrl);

export function App() {
  return <AppShell metadata={appMetadata} apiConfigured={apiClient.isConfigured} />;
}

