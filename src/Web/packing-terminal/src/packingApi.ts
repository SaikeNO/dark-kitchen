import { getJson, postJson } from "./api/http";
import type { PackingManifest } from "./packingTypes";

export function listManifests(signal?: AbortSignal) {
  return getJson<PackingManifest[]>("/api/packing/manifests", signal);
}

export function issueManifest(request: { readonly manifestId: string; readonly pickupCode: string }) {
  return postJson<PackingManifest>(
    `/api/packing/manifests/${request.manifestId}/issued`,
    { pickupCode: request.pickupCode }
  );
}
