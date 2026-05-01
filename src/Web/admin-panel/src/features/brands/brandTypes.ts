export interface Brand {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly logoUrl: string | null;
  readonly isActive: boolean;
}

export interface BrandPayload {
  readonly name: string;
  readonly description: string;
  readonly logoUrl: string;
  readonly isActive: boolean;
}
