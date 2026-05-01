import { getJson, postJson, putJson } from "../../api/http";
import type { Ingredient, IngredientPayload, Recipe, RecipePayload } from "./recipeTypes";

export function listIngredients(signal?: AbortSignal) {
  return getJson<Ingredient[]>("/api/admin/ingredients", signal);
}

export function saveIngredient(ingredientId: string | null, payload: IngredientPayload) {
  return ingredientId === null
    ? postJson<Ingredient>("/api/admin/ingredients", payload)
    : putJson<Ingredient>(`/api/admin/ingredients/${ingredientId}`, payload);
}

export function deactivateIngredient(ingredientId: string) {
  return postJson<unknown>(`/api/admin/ingredients/${ingredientId}/deactivate`);
}

export function getRecipe(productId: string, signal?: AbortSignal) {
  return getJson<Recipe>(`/api/admin/products/${productId}/recipe`, signal);
}

export function saveRecipe(productId: string, payload: RecipePayload) {
  return putJson<Recipe>(`/api/admin/products/${productId}/recipe`, payload);
}
