import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import { createElement } from "react";
import { renderToString } from "react-dom/server";
import { MemoryRouter } from "react-router-dom";
import { App } from "./App";
import { appMetadata } from "./appMetadata";

describe("Admin Panel metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Admin Panel");
  });

  it("renders the admin shell heading before auth resolves", () => {
    const queryClient = new QueryClient();

    expect(renderToString(
      createElement(QueryClientProvider, { client: queryClient },
        createElement(MemoryRouter, null, createElement(App))
      )
    )).toContain("Admin Panel");
  });
});
