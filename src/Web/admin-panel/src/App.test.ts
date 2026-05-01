import { describe, expect, it } from "vitest";
import { createElement } from "react";
import { renderToString } from "react-dom/server";
import { App } from "./App";
import { appMetadata } from "./appMetadata";

describe("Admin Panel metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Admin Panel");
  });

  it("renders the admin shell heading before auth resolves", () => {
    expect(renderToString(createElement(App))).toContain("Admin Panel");
  });
});
