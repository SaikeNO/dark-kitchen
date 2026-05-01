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
