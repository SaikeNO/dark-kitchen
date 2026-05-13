export type ManifestStatus = "Waiting" | "ReadyForPacking" | "Delayed" | "Issued";
export type ConnectionStatus = "offline" | "connecting" | "connected";

export interface ManifestItem {
  readonly id: string;
  readonly orderItemId: string;
  readonly menuItemId: string;
  readonly itemName: string;
  readonly quantity: number;
  readonly isReady: boolean;
  readonly completedAt: string | null;
}

export interface PackingManifest {
  readonly id: string;
  readonly orderId: string;
  readonly brandId: string;
  readonly sourceChannel: string;
  readonly status: ManifestStatus;
  readonly totalItemsCount: number;
  readonly readyItemsCount: number;
  readonly isDelayed: boolean;
  readonly createdAt: string;
  readonly updatedAt: string;
  readonly readyForPackingAt: string | null;
  readonly issuedAt: string | null;
  readonly pickupCode: string;
  readonly items: readonly ManifestItem[];
}
