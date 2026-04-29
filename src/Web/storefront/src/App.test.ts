import { describe, expect, it } from "vitest";
import { appMetadata } from "./appMetadata";

describe("Storefront metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Storefront");
  });
});

