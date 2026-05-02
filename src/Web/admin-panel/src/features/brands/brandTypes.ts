export interface Brand {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly logoUrl: string | null;
  readonly domains: readonly string[];
  readonly heroTitle: string | null;
  readonly heroSubtitle: string | null;
  readonly primaryColor: string;
  readonly accentColor: string;
  readonly backgroundColor: string;
  readonly textColor: string;
  readonly isActive: boolean;
}

export interface BrandPayload {
  readonly name: string;
  readonly description: string;
  readonly logoUrl: string;
  readonly domains: readonly string[];
  readonly heroTitle: string;
  readonly heroSubtitle: string;
  readonly primaryColor: string;
  readonly accentColor: string;
  readonly backgroundColor: string;
  readonly textColor: string;
  readonly isActive: boolean;
}
