import { createApiClient } from "@dark-kitchen/api-client";
import { clientConfig } from "@dark-kitchen/config";
import axios from "axios";

const apiClient = createApiClient(clientConfig.apiBaseUrl);

export const apiConfigured = apiClient.isConfigured;

export interface Session {
  readonly email: string;
  readonly roles: string[];
}

export interface Brand {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly logoUrl: string | null;
  readonly isActive: boolean;
}

export interface Category {
  readonly id: string;
  readonly brandId: string;
  readonly name: string;
  readonly sortOrder: number;
  readonly isActive: boolean;
}

export interface Product {
  readonly id: string;
  readonly brandId: string;
  readonly categoryId: string;
  readonly name: string;
  readonly description: string | null;
  readonly price: number;
  readonly currency: string;
  readonly isActive: boolean;
  readonly stationId: string | null;
  readonly stationCode: string | null;
  readonly recipeItemCount: number;
}

export interface Ingredient {
  readonly id: string;
  readonly name: string;
  readonly unit: string;
  readonly isActive: boolean;
}

export interface Station {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
  readonly isActive: boolean;
}

export interface Recipe {
  readonly productId: string;
  readonly items: RecipeItem[];
}

export interface RecipeItem {
  readonly ingredientId: string;
  readonly ingredientName: string;
  readonly unit: string;
  readonly quantity: number;
}

export interface CatalogSnapshot {
  readonly brands: Brand[];
  readonly categories: Category[];
  readonly products: Product[];
  readonly ingredients: Ingredient[];
  readonly stations: Station[];
}

export interface BrandPayload {
  readonly name: string;
  readonly description: string;
  readonly logoUrl: string;
  readonly isActive: boolean;
}

export interface CategoryPayload {
  readonly brandId: string;
  readonly name: string;
  readonly sortOrder: number;
  readonly isActive: boolean;
}

export interface ProductPayload {
  readonly brandId: string;
  readonly categoryId: string;
  readonly name: string;
  readonly description: string;
  readonly price: number;
  readonly currency: string;
}

export interface IngredientPayload {
  readonly name: string;
  readonly unit: string;
  readonly isActive: boolean;
}

export interface StationPayload {
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
  readonly isActive: boolean;
}

export interface RecipePayload {
  readonly items: readonly {
    readonly ingredientId: string;
    readonly quantity: number;
  }[];
}

interface ApiProblem {
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Record<string, string[]>;
}

export class ApiRequestError extends Error {
  constructor(message: string, readonly status: number | undefined) {
    super(message);
    this.name = "ApiRequestError";
  }
}

const adminApi = axios.create({
  baseURL: apiClient.baseUrl.length > 0 ? apiClient.baseUrl : undefined,
  timeout: 10_000,
  withCredentials: true
});

adminApi.interceptors.response.use(
  response => response,
  (error: unknown) => {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const problem = asApiProblem(error.response?.data);
      const fallback = status === undefined
        ? "Network request failed."
        : `Request failed with ${status}.`;

      return Promise.reject(new ApiRequestError(problemMessage(problem, fallback), status));
    }

    return Promise.reject(error instanceof Error ? error : new Error("Unexpected API error."));
  }
);

export async function getCurrentSession(signal?: AbortSignal) {
  try {
    const session = await getJson<unknown>("/api/admin/auth/me", signal);

    return isSession(session) ? session : null;
  } catch (error) {
    if (error instanceof ApiRequestError) {
      return null;
    }

    throw error;
  }
}

export async function loginAdmin(email: string, password: string) {
  const session = await postJson<unknown>("/api/admin/auth/login", { email, password });

  if (!isSession(session)) {
    throw new ApiRequestError("Invalid session response.", undefined);
  }

  return session;
}

export function logoutAdmin() {
  return postJson<void>("/api/admin/auth/logout");
}

export async function getCatalogSnapshot(signal?: AbortSignal): Promise<CatalogSnapshot> {
  const [brands, categories, products, ingredients, stations] = await Promise.all([
    getJson<Brand[]>("/api/admin/brands", signal),
    getJson<Category[]>("/api/admin/categories", signal),
    getJson<Product[]>("/api/admin/products", signal),
    getJson<Ingredient[]>("/api/admin/ingredients", signal),
    getJson<Station[]>("/api/admin/stations", signal)
  ]);

  return { brands, categories, products, ingredients, stations };
}

export function getRecipe(productId: string, signal?: AbortSignal) {
  return getJson<Recipe>(`/api/admin/products/${productId}/recipe`, signal);
}

export function saveBrand(brandId: string | null, payload: BrandPayload) {
  return brandId === null
    ? postJson<Brand>("/api/admin/brands", payload)
    : putJson<Brand>(`/api/admin/brands/${brandId}`, payload);
}

export function saveCategory(categoryId: string | null, payload: CategoryPayload) {
  return categoryId === null
    ? postJson<Category>("/api/admin/categories", payload)
    : putJson<Category>(`/api/admin/categories/${categoryId}`, payload);
}

export function saveProduct(productId: string | null, payload: ProductPayload) {
  return productId === null
    ? postJson<Product>("/api/admin/products", payload)
    : putJson<Product>(`/api/admin/products/${productId}`, payload);
}

export function saveIngredient(ingredientId: string | null, payload: IngredientPayload) {
  return ingredientId === null
    ? postJson<Ingredient>("/api/admin/ingredients", payload)
    : putJson<Ingredient>(`/api/admin/ingredients/${ingredientId}`, payload);
}

export function saveStation(stationId: string | null, payload: StationPayload) {
  return stationId === null
    ? postJson<Station>("/api/admin/stations", payload)
    : putJson<Station>(`/api/admin/stations/${stationId}`, payload);
}

export function deactivateBrand(brandId: string) {
  return postJson<unknown>(`/api/admin/brands/${brandId}/deactivate`);
}

export function deactivateCategory(categoryId: string) {
  return postJson<unknown>(`/api/admin/categories/${categoryId}/deactivate`);
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

export function deactivateIngredient(ingredientId: string) {
  return postJson<unknown>(`/api/admin/ingredients/${ingredientId}/deactivate`);
}

export function saveRecipe(productId: string, payload: RecipePayload) {
  return putJson<Recipe>(`/api/admin/products/${productId}/recipe`, payload);
}

export function deactivateStation(stationId: string) {
  return postJson<unknown>(`/api/admin/stations/${stationId}/deactivate`);
}

async function getJson<TResponse>(url: string, signal?: AbortSignal) {
  const response = await adminApi.get<TResponse>(url, { signal });
  return response.data;
}

async function postJson<TResponse>(url: string, data?: unknown) {
  const response = await adminApi.post<TResponse>(url, data);
  return response.data;
}

async function putJson<TResponse>(url: string, data: unknown) {
  const response = await adminApi.put<TResponse>(url, data);
  return response.data;
}

function asApiProblem(data: unknown): ApiProblem | undefined {
  if (typeof data !== "object" || data === null) {
    return undefined;
  }

  return data;
}

function isSession(value: unknown): value is Session {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as Partial<Session>;

  return typeof candidate.email === "string" && Array.isArray(candidate.roles);
}

function problemMessage(problem: ApiProblem | undefined, fallback: string) {
  if (problem?.errors !== undefined) {
    return Object.entries(problem.errors)
      .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
      .join(" ");
  }

  return problem?.detail ?? problem?.title ?? fallback;
}
