import { expect, test } from "@playwright/test";

test.describe("Storefront", () => {
  test("shows brand picker on root page", async ({ page }) => {
    await page.goto("http://127.0.0.1:5174");

    await expect(page.getByRole("heading", { name: "Wybierz marke" })).toBeVisible();
    await expect(page.getByRole("link", { name: /Burger Ghost/ })).toBeVisible();
    await expect(page.getByRole("button", { name: "Zmien motyw" })).toBeVisible();
  });
});
