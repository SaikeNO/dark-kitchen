import { getJson, postJson } from "./api/http";
import type { PackingManifest } from "./packingTypes";

export function listManifests(signal?: AbortSignal) {
  return getJson<PackingManifest[]>("/api/packing/manifests", signal);
}

export function issueManifest(manifestId: string) {
  return postJson<PackingManifest>(`/api/packing/manifests/${manifestId}/issued`);
}
