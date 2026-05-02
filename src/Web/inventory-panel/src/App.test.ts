import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createElement } from "react";
import { renderToString } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { App } from "./App";

describe("Inventory Panel", () => {
  it("renders the shell title", () => {
    const queryClient = new QueryClient();

    expect(renderToString(
      createElement(QueryClientProvider, { client: queryClient }, createElement(App))
    )).toContain("Magazyn");
  });
});
