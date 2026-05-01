export const queryKeys = {
  session: ["admin", "session"] as const,
  brands: ["catalog", "brands"] as const,
  categories: ["catalog", "categories"] as const,
  products: ["catalog", "products"] as const,
  ingredients: ["catalog", "ingredients"] as const,
  stations: ["catalog", "stations"] as const,
  recipe: (productId: string) => ["catalog", "recipes", productId] as const
};
