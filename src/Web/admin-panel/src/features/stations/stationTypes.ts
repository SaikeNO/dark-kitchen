export interface Station {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
  readonly isActive: boolean;
}

export interface StationPayload {
  readonly code: string;
  readonly name: string;
  readonly displayColor: string;
  readonly isActive: boolean;
}
