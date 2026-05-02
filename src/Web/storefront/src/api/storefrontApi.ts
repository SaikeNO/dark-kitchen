import { getJson, patchJson, postJson } from "./http";

export interface Theme {
  readonly primaryColor: string;
  readonly accentColor: string;
  readonly backgroundColor: string;
  readonly textColor: string;
}

export interface BrandContext {
  readonly brandId: string;
  readonly brandName: string;
  readonly description: string | null;
  readonly logoUrl: string | null;
  readonly heroTitle: string | null;
  readonly heroSubtitle: string | null;
  readonly theme: Theme;
}

export interface MenuResponse {
  readonly brand: BrandContext;
  readonly categories: readonly MenuCategory[];
}

export interface MenuCategory {
  readonly id: string;
  readonly name: string;
  readonly sortOrder: number;
  readonly products: readonly MenuProduct[];
}

export interface MenuProduct {
  readonly id: string;
  readonly categoryId: string;
  readonly name: string;
  readonly description: string | null;
  readonly imageUrl: string | null;
  readonly price: number;
  readonly currency: string;
}

export interface CartLine {
  readonly menuItemId: string;
  readonly quantity: number;
}

export interface CartResponse {
  readonly cartId: string;
  readonly brandId: string;
  readonly totalPrice: number;
  readonly currency: string;
  readonly items: readonly CartItemResponse[];
}

export interface CartItemResponse {
  readonly menuItemId: string;
  readonly name: string;
  readonly imageUrl: string | null;
  readonly quantity: number;
  readonly unitPrice: number;
  readonly currency: string;
  readonly lineTotal: number;
}

export interface Session {
  readonly id: string;
  readonly email: string;
  readonly displayName: string | null;
  readonly phone: string | null;
}

export interface CheckoutResponse {
  readonly paymentId: string;
  readonly paymentStatus: string;
  readonly orderId: string | null;
  readonly correlationId: string | null;
  readonly failureReason: string | null;
}

export interface CustomerForm {
  readonly displayName: string;
  readonly phone: string;
  readonly deliveryNote: string;
}

export function listBrands(signal?: AbortSignal) {
  return getJson<BrandContext[]>("/api/storefront/brands", signal);
}

export function getMenu(brandId: string, signal?: AbortSignal) {
  return getJson<MenuResponse>(`/api/storefront/menu?brandId=${encodeURIComponent(brandId)}`, signal);
}

export function getSession(signal?: AbortSignal) {
  return getJson<Session | null>("/api/storefront/auth/me", signal);
}

export function createCart(brandId: string, cartId: string | null) {
  return postJson<CartResponse>(`/api/storefront/carts?brandId=${encodeURIComponent(brandId)}`, { cartId });
}

export function updateCart(brandId: string, cartId: string, items: readonly CartLine[]) {
  return patchJson<CartResponse>(
    `/api/storefront/carts/${cartId}?brandId=${encodeURIComponent(brandId)}`,
    { items }
  );
}

export function checkout(brandId: string, cartId: string, customer: CustomerForm, mockPaymentResult: "success" | "failed") {
  return postJson<CheckoutResponse>(`/api/storefront/checkout?brandId=${encodeURIComponent(brandId)}`, {
    cartId,
    customer,
    mockPaymentResult
  });
}

export function login(email: string, password: string) {
  return postJson<Session>("/api/storefront/auth/login", { email, password });
}

export function register(email: string, password: string, displayName: string, phone: string) {
  return postJson<Session>("/api/storefront/auth/register", { email, password, displayName, phone });
}

export function logout() {
  return postJson<void>("/api/storefront/auth/logout", {});
}
