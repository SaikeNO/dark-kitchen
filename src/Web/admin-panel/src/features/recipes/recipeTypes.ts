export interface Ingredient {
  readonly id: string;
  readonly name: string;
  readonly unit: string;
  readonly isActive: boolean;
}

export interface IngredientPayload {
  readonly name: string;
  readonly unit: string;
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

export interface RecipePayload {
  readonly items: readonly {
    readonly ingredientId: string;
    readonly quantity: number;
  }[];
}
