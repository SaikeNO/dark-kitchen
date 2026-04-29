import { describe, expect, it } from "vitest";
import { appMetadata } from "./appMetadata";

describe("Packing Terminal metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Packing Terminal");
  });
});

