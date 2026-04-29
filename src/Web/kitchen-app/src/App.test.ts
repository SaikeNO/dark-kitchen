import { describe, expect, it } from "vitest";
import { appMetadata } from "./appMetadata";

describe("Kitchen App metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Kitchen App");
  });
});

