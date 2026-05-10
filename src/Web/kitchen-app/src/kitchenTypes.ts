export interface Station {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
}

export type KitchenTaskStatus = "Pending" | "InProgress" | "Done" | "RoutingMissing";

export interface KitchenTask {
  readonly id: string;
  readonly ticketId: string;
  readonly orderId: string;
  readonly orderItemId: string;
  readonly menuItemId: string;
  readonly itemName: string;
  readonly quantity: number;
  readonly stationId: string;
  readonly stationCode: string;
  readonly status: KitchenTaskStatus;
  readonly createdAt: string;
  readonly startedAt: string | null;
  readonly completedAt: string | null;
}

export type ConnectionStatus = "connected" | "connecting" | "offline";
