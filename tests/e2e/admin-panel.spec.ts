import type { Page } from "@playwright/test";
import { ADMIN_URL, cardByText, expect, loginAsManager, loginAsOperator, mainHeading, openAdminPanel, test, uniqueName } from "./support/adminPanel";

const seededBrandId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001";

test.describe("Admin Panel", () => {
  test("authenticates manager and logs out", async ({ page }) => {
    await openAdminPanel(page);
    await expect(page.getByRole("button", { name: "Sign in" })).toBeVisible();

    await page.getByRole("button", { name: "Manager demo" }).click();
    await page.getByRole("button", { name: "Sign in" }).click();

    await expect(page.getByText("manager@darkkitchen.local", { exact: true })).toBeVisible();
    await expect(page.getByText("Manager", { exact: true })).toBeVisible();
    await expect(page.getByRole("link", { exact: true, name: "Menu" })).toBeVisible();
    await expect(page.getByRole("link", { exact: true, name: "Receptury" })).toBeVisible();

    await page.getByRole("button", { name: "Logout" }).click();
    await expect(page.getByRole("heading", { level: 2, name: "Sign in" })).toBeVisible();
  });

  test("keeps user on login screen after invalid credentials", async ({ page }) => {
    await openAdminPanel(page);

    await page.getByLabel("Email").fill("manager@darkkitchen.local");
    await page.getByLabel("Password").fill("Wrong123!");
    const loginResponse = page.waitForResponse(response => response.url().includes("/api/admin/auth/login"));
    await page.getByRole("button", { name: "Sign in" }).click();

    expect((await loginResponse).status()).toBe(401);
    await expect(page.getByRole("heading", { level: 2, name: "Sign in" })).toBeVisible();
    await expect(page.getByText(/401/)).toBeVisible();
  });

  test("enforces operator read-only mode", async ({ page }) => {
    await loginAsOperator(page);

    await expect(page.getByText("Tryb operatora: odczyt bez zapisu.")).toBeVisible();
    await expect(page.getByLabel("Nazwa", { exact: true })).toBeDisabled();
    await expect(page.getByLabel("Opis", { exact: true })).toBeDisabled();
    await expect(page.getByRole("button", { name: "Zapisz" })).toBeDisabled();
    await expect(cardByText(page, "Burger Ghost").getByRole("button", { name: "Dezaktywuj" })).toBeDisabled();

    await page.goto(`${ADMIN_URL}/menu/${seededBrandId}/products`);
    await expect(page.getByRole("heading", { name: "Menu: Burger Ghost" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Dodaj produkt" })).toBeDisabled();

    await page.goto(`${ADMIN_URL}/menu/${seededBrandId}/products/new`);
    await expect(page.getByRole("heading", { name: "Nowy produkt" })).toBeVisible();
    await expect(page.getByLabel("Nazwa", { exact: true })).toBeDisabled();
    await expect(page.getByRole("button", { name: "Zapisz produkt" })).toBeDisabled();
  });

  test("allows manager to manage brands and stations", async ({ page }) => {
    await loginAsManager(page);

    const brandName = uniqueName("Brand");
    const editedBrandName = `${brandName} edited`;
    await createBrand(page, brandName, "Brand created by Playwright E2E");

    await cardByText(page, brandName).getByRole("button", { name: "Edytuj" }).click();
    await expect(page.getByRole("heading", { name: "Edycja marki" })).toBeVisible();
    await page.getByLabel("Nazwa", { exact: true }).fill(editedBrandName);
    await page.getByRole("button", { name: "Zapisz" }).click();

    const editedBrandCard = cardByText(page, editedBrandName);
    await expect(editedBrandCard).toBeVisible();
    await editedBrandCard.getByRole("button", { name: "Dezaktywuj" }).click();
    await expect(editedBrandCard.getByText("Nieaktywne")).toBeVisible();
    await editedBrandCard.getByRole("button", { name: "Aktywuj" }).click();
    await expect(editedBrandCard.getByText("Aktywne")).toBeVisible();

    await page.getByRole("link", { name: "Stacje" }).click();
    await expect(mainHeading(page, "Stacje")).toBeVisible();

    const stationCode = uniqueStationCode();
    const stationName = uniqueName("Station");
    const editedStationName = `${stationName} edited`;
    await createStation(page, stationCode, stationName);

    const stationCard = cardByText(page, stationCode);
    await stationCard.getByRole("button", { name: "Edytuj" }).click();
    await expect(page.getByRole("heading", { name: "Edycja stacji" })).toBeVisible();
    await page.getByLabel("Nazwa", { exact: true }).fill(editedStationName);
    await page.getByRole("button", { name: "Zapisz" }).click();

    await expect(stationCard.getByText(editedStationName)).toBeVisible();
    await stationCard.getByRole("button", { name: "Dezaktywuj" }).click();
    await expect(stationCard.getByText("Nieaktywne")).toBeVisible();
  });

  test("uploads brand logo without restarting the app", async ({ page }) => {
    await loginAsManager(page);
    await page.getByRole("link", { name: "Marki" }).click();
    await cardByText(page, "Burger Ghost").getByRole("button", { name: "Edytuj" }).click();

    const uploadResponse = page.waitForResponse(response =>
      response.url().includes("/api/admin/uploads/brand-logo") && response.status() === 201);
    await page.getByLabel("Wgraj logo").setInputFiles({
      name: "brand-logo.png",
      mimeType: "image/png",
      buffer: Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=", "base64")
    });

    await uploadResponse;
    await expect(page.getByText("Wgrano logo.")).toBeVisible();
    await expect(page.getByRole("heading", { name: "Edycja marki" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Zapisz" })).toBeEnabled();
  });

  test("creates menu item, recipe, and toggles product status", async ({ page }) => {
    await loginAsManager(page);

    const brandName = uniqueName("Menu Brand");
    const categoryName = uniqueName("Category");
    const stationCode = uniqueStationCode();
    const stationName = uniqueName("Kitchen Station");
    const productName = uniqueName("Product");
    const ingredientName = uniqueName("Ingredient");

    await createBrand(page, brandName, "Menu flow brand");
    await createStation(page, stationCode, stationName);
    await openBrandProducts(page, brandName);
    await createCategory(page, brandName, categoryName);
    await createProduct(page, brandName, categoryName, stationCode, stationName, productName);

    const productCard = cardByText(page, productName);
    await expect(productCard.getByText(categoryName)).toBeVisible();
    await expect(productCard.getByText("45.50 PLN")).toBeVisible();
    await expect(productCard.getByText(`Stacja: ${stationCode}`)).toBeVisible();
    await expect(productCard.getByText("Nieaktywne")).toBeVisible();

    await page.getByLabel("Szukaj", { exact: true }).fill(productName);
    await page.getByLabel("Status").selectOption("inactive");
    await page.getByLabel("Kategoria").selectOption({ label: categoryName });
    await expect(productCard).toBeVisible();

    await page.getByLabel("Szukaj", { exact: true }).fill("missing product");
    await expect(page.getByText("Brak wyników")).toBeVisible();
    await page.getByLabel("Szukaj", { exact: true }).fill(productName);
    await expect(productCard).toBeVisible();

    await createIngredient(page, ingredientName);
    await createRecipe(page, productName, ingredientName);

    await openBrandProducts(page, brandName);
    await page.getByLabel("Szukaj", { exact: true }).fill(productName);
    await page.getByLabel("Status").selectOption("all");
    const completedProductCard = cardByText(page, productName);

    await completedProductCard.getByRole("button", { name: "Aktywuj" }).click();
    await expect(completedProductCard.getByText("Aktywne")).toBeVisible();
    await completedProductCard.getByRole("button", { name: "Dezaktywuj" }).click();
    await expect(completedProductCard.getByText("Nieaktywne")).toBeVisible();
  });

  test("persists theme toggle across reload", async ({ page }) => {
    await openAdminPanel(page);

    const toggle = page.getByRole("button", { name: /^(Dark|Light)$/ });
    const initialLabel = (await toggle.innerText()).trim();
    const expectedLabel = initialLabel === "Dark" ? "Light" : "Dark";

    await toggle.click();
    await expect(page.getByRole("button", { name: expectedLabel })).toBeVisible();

    await page.reload();
    await expect(page.getByRole("button", { name: expectedLabel })).toBeVisible();
  });

  test("supports mobile navigation", async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await loginAsManager(page);

    await page.getByRole("button", { name: "Menu" }).click();
    await page.getByRole("link", { name: "Stacje" }).click();

    await expect(mainHeading(page, "Stacje")).toBeVisible();
  });
});

async function createBrand(page: Page, name: string, description: string) {
  await page.getByRole("link", { name: "Marki" }).click();
  await expect(mainHeading(page, "Marki")).toBeVisible();
  await page.getByLabel("Nazwa", { exact: true }).fill(name);
  await page.getByLabel("Opis", { exact: true }).fill(description);
  await page.getByRole("button", { name: "Zapisz" }).click();
  await expect(cardByText(page, name)).toBeVisible();
  await expect(page.getByRole("link", { name })).toBeVisible();
}

async function createStation(page: Page, code: string, name: string) {
  await page.getByRole("link", { name: "Stacje" }).click();
  await expect(mainHeading(page, "Stacje")).toBeVisible();
  await page.getByLabel("Kod").fill(code);
  await page.getByLabel("Nazwa", { exact: true }).fill(name);
  await page.getByLabel("Kolor").fill("#ef4444");
  await page.getByRole("button", { name: "Zapisz" }).click();
  await expect(cardByText(page, code)).toBeVisible();
}

async function openBrandProducts(page: Page, brandName: string) {
  await page.getByRole("link", { name: brandName }).click();
  await expect(page.getByRole("heading", { name: `Menu: ${brandName}` })).toBeVisible();
}

async function createCategory(page: Page, brandName: string, categoryName: string) {
  await page.getByRole("button", { name: "Kategorie" }).click();
  await expect(page.getByRole("heading", { name: `Kategorie: ${brandName}` })).toBeVisible();
  await page.getByRole("button", { name: "Dodaj kategorię" }).click();
  await expect(page.getByRole("heading", { name: "Nowa kategoria" })).toBeVisible();
  await page.getByLabel("Nazwa", { exact: true }).fill(categoryName);
  await page.getByLabel("Kolejność").fill("20");
  await page.getByRole("button", { name: "Zapisz kategorię" }).click();
  await expect(cardByText(page, categoryName)).toBeVisible();
}

async function createProduct(
  page: Page,
  brandName: string,
  categoryName: string,
  stationCode: string,
  stationName: string,
  productName: string
) {
  await page.getByRole("button", { name: "Produkty" }).click();
  await expect(page.getByRole("heading", { name: `Menu: ${brandName}` })).toBeVisible();
  await page.getByRole("button", { name: "Dodaj produkt" }).click();
  await expect(page.getByRole("heading", { name: "Nowy produkt" })).toBeVisible();

  await page.getByLabel("Kategoria").selectOption({ label: categoryName });
  await page.getByLabel("Nazwa", { exact: true }).fill(productName);
  await page.getByLabel("Opis", { exact: true }).fill("Product created by Playwright E2E");
  await page.getByLabel("Cena").fill("45.50");
  await page.getByLabel("Waluta").fill("PLN");
  await page.getByLabel("Stacja").selectOption({ label: `${stationCode} - ${stationName}` });
  await page.getByRole("button", { name: "Zapisz produkt" }).click();

  await expect(page.getByRole("heading", { name: `Menu: ${brandName}` })).toBeVisible();
  await expect(cardByText(page, productName)).toBeVisible();
}

async function createIngredient(page: Page, ingredientName: string) {
  await page.getByRole("link", { name: "Składniki" }).click();
  await expect(page.getByRole("heading", { name: "Składniki" })).toBeVisible();
  await page.getByRole("button", { name: "Dodaj składnik" }).click();
  await page.getByLabel("Nazwa", { exact: true }).fill(ingredientName);
  await page.getByLabel("Jednostka").fill("g");
  await page.getByRole("button", { name: "Zapisz składnik" }).click();
  await expect(cardByText(page, ingredientName)).toBeVisible();
}

async function createRecipe(page: Page, productName: string, ingredientName: string) {
  await page.getByRole("link", { name: "Receptury produktów" }).click();
  await expect(page.getByRole("heading", { name: "Receptury produktów" })).toBeVisible();
  await page.getByLabel("Szukaj produktu").fill(productName);
  await page.getByRole("button").filter({ hasText: productName }).click();
  await expect(page.getByRole("heading", { name: productName })).toBeVisible();

  await page.getByRole("button", { name: "Dodaj składnik" }).click();
  await page.getByLabel("Składnik").selectOption({ label: ingredientName });
  await page.getByLabel("Ilość").fill("0");
  await page.getByRole("button", { name: "Zapisz recepturę" }).click();
  await expect(page.getByText("Uzupełnij składnik i ilość większą od zera.")).toBeVisible();

  await page.getByLabel("Ilość").fill("2.5");
  await page.getByRole("button", { name: "Zapisz recepturę" }).click();
  await expect(page.getByText("Zapisano recepturę.")).toBeVisible();
  await expect(page.getByRole("button").filter({ hasText: productName })).toContainText("1 pozycji receptury");
}

function uniqueStationCode() {
  return `E2E${Math.random().toString(36).slice(2, 7).toUpperCase()}`;
}
