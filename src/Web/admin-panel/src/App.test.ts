import { describe, expect, it } from "vitest";
import { appMetadata } from "./appMetadata";

describe("Admin Panel metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Admin Panel");
  });
});

