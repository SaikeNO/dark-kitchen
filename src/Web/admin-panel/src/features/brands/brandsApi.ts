import { getJson, postJson, putJson } from "../../api/http";
import type { Brand, BrandPayload } from "./brandTypes";

export function listBrands(signal?: AbortSignal) {
  return getJson<Brand[]>("/api/admin/brands", signal);
}

export function saveBrand(brandId: string | null, payload: BrandPayload) {
  return brandId === null
    ? postJson<Brand>("/api/admin/brands", payload)
    : putJson<Brand>(`/api/admin/brands/${brandId}`, payload);
}

export function deactivateBrand(brandId: string) {
  return postJson<unknown>(`/api/admin/brands/${brandId}/deactivate`);
}
