import { getJson, postJson, putJson } from "../../api/http";
import type { Category, CategoryPayload, Product, ProductPayload } from "./menuTypes";

export function listCategories(signal?: AbortSignal) {
  return getJson<Category[]>("/api/admin/categories", signal);
}

export function saveCategory(categoryId: string | null, payload: CategoryPayload) {
  return categoryId === null
    ? postJson<Category>("/api/admin/categories", payload)
    : putJson<Category>(`/api/admin/categories/${categoryId}`, payload);
}

export function deactivateCategory(categoryId: string) {
  return postJson<unknown>(`/api/admin/categories/${categoryId}/deactivate`);
}

export function listProducts(signal?: AbortSignal) {
  return getJson<Product[]>("/api/admin/products", signal);
}

export function saveProduct(productId: string | null, payload: ProductPayload) {
  return productId === null
    ? postJson<Product>("/api/admin/products", payload)
    : putJson<Product>(`/api/admin/products/${productId}`, payload);
}

export function activateProduct(productId: string) {
  return postJson<Product>(`/api/admin/products/${productId}/activate`);
}

export function deactivateProduct(productId: string) {
  return postJson<unknown>(`/api/admin/products/${productId}/deactivate`);
}

export function saveProductStationRoute(productId: string, stationId: string) {
  return putJson<unknown>(`/api/admin/products/${productId}/station-route`, { stationId });
}
