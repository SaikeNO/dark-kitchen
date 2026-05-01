export interface Session {
  readonly email: string;
  readonly roles: string[];
}

export function isSession(value: unknown): value is Session {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as Partial<Session>;

  return typeof candidate.email === "string" && Array.isArray(candidate.roles);
}
