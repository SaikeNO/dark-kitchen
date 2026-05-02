export interface InventoryItem {
  readonly ingredientId: string;
  readonly name: string;
  readonly unit: string;
  readonly onHandQuantity: number;
  readonly reservedQuantity: number;
  readonly availableQuantity: number;
  readonly minSafetyLevel: number;
  readonly isBelowSafetyLevel: boolean;
  readonly reorderQuantity: number;
}

export interface DeliveryPayload {
  readonly quantity: number;
  readonly note?: string;
}

export interface AdjustmentPayload {
  readonly onHandQuantity: number;
  readonly minSafetyLevel?: number;
  readonly note?: string;
}
