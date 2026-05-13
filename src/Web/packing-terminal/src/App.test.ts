import { describe, expect, it } from "vitest";
import { appMetadata } from "./appMetadata";
import { applyManifestUpdate, groupManifests } from "./manifestCache";
import type { PackingManifest } from "./packingTypes";

describe("Packing Terminal metadata", () => {
  it("names the application", () => {
    expect(appMetadata.name).toBe("Packing Terminal");
  });
});

describe("packing manifest cache", () => {
  it("groups waiting, ready and delayed manifests", () => {
    const groups = groupManifests([
      manifest("1", "Waiting"),
      manifest("2", "ReadyForPacking"),
      manifest("3", "Delayed", true)
    ]);

    expect(groups.waiting).toHaveLength(1);
    expect(groups.ready).toHaveLength(1);
    expect(groups.delayed).toHaveLength(1);
  });

  it("removes issued manifest updates from active cache", () => {
    const current = [manifest("1", "ReadyForPacking")];
    const next = applyManifestUpdate(current, manifest("1", "Issued"));

    expect(next).toEqual([]);
  });

  it("places delayed manifests before ready and waiting entries", () => {
    const next = applyManifestUpdate(
      [manifest("waiting", "Waiting"), manifest("ready", "ReadyForPacking")],
      manifest("delayed", "Delayed", true));

    expect(next.map(item => item.id)).toEqual(["delayed", "ready", "waiting"]);
  });
});

function manifest(
  id: string,
  status: PackingManifest["status"],
  isDelayed = false): PackingManifest
{
  return {
    id,
    orderId: `${id}-order`,
    brandId: `${id}-brand`,
    sourceChannel: "test",
    status,
    totalItemsCount: 1,
    readyItemsCount: status === "Waiting" ? 0 : 1,
    isDelayed,
    createdAt: `2026-05-13T00:00:0${id.length}Z`,
    updatedAt: `2026-05-13T00:00:0${id.length}Z`,
    readyForPackingAt: null,
    issuedAt: null,
    items: []
  };
}
