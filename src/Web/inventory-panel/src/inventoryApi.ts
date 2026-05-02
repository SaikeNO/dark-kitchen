import { getJson, postJson } from "./api/http";
import type { AdjustmentPayload, DeliveryPayload, InventoryItem } from "./inventoryTypes";

export function listInventoryItems(signal?: AbortSignal) {
  return getJson<InventoryItem[]>("/api/admin/inventory/items", signal);
}

export function listShortages(signal?: AbortSignal) {
  return getJson<InventoryItem[]>("/api/admin/inventory/shortages", signal);
}

export function recordDelivery(ingredientId: string, payload: DeliveryPayload) {
  return postJson<InventoryItem>(`/api/admin/inventory/items/${ingredientId}/delivery`, payload);
}

export function adjustInventoryItem(ingredientId: string, payload: AdjustmentPayload) {
  return postJson<InventoryItem>(`/api/admin/inventory/items/${ingredientId}/adjustment`, payload);
}
