import { expect, test } from "@playwright/test";
import type { Locator, Page } from "@playwright/test";

const storefrontUrl = "http://127.0.0.1:5174";
const kitchenUrl = "http://127.0.0.1:5175";
const packingUrl = "http://127.0.0.1:5176";

test("order flows from storefront through kitchen and packing", async ({ page }) => {
  await page.goto(storefrontUrl);
  await page.getByRole("link", { name: /Burger Ghost/ }).click();

  await expect(page.getByRole("heading", { name: "Burger Ghost" })).toBeVisible();
  await page.getByRole("button", { name: "Dodaj" }).click();
  await page.getByLabel("Imie").fill("Demo Customer");
  await page.getByLabel("Telefon").fill("500600700");
  await page.getByLabel("Notatka").fill("E2E");
  await page.getByRole("button", { name: /Zamow za/ }).click();

  const orderResult = page.locator(".result.success");
  await expect(orderResult).toContainText(/Order [0-9a-f-]{36}/i);
  const orderText = await orderResult.textContent();
  const orderId = orderText?.match(/[0-9a-f-]{36}/i)?.[0];
  expect(orderId).toBeTruthy();
  const shortOrderId = orderId!.slice(0, 8);

  await page.goto(kitchenUrl);
  await page.getByRole("button", { name: /Grill/ }).click();
  await expect(page.getByRole("heading", { name: "Grill" })).toBeVisible();

  const taskCard = page.locator(".task-card").filter({ hasText: shortOrderId });
  await expect(taskCard).toBeVisible();

  await page.reload();
  await expect(page.getByRole("heading", { name: "Grill" })).toBeVisible();
  await expect(taskCard).toBeVisible();

  await taskCard.getByRole("button", { name: "Start" }).click();
  await expect(taskCard.getByRole("button", { name: "Done" })).toBeVisible();
  await taskCard.getByRole("button", { name: "Done" }).click();
  await expect(taskCard).toBeHidden();

  await page.goto(packingUrl);
  const manifestCard = page.locator(".manifest-card").filter({ hasText: shortOrderId });
  await expect(manifestCard).toBeVisible();
  await refreshPackingUntilReady(page, manifestCard);
  await expect(manifestCard).toContainText(/1\/1 gotowe/);
  await manifestCard.getByRole("button", { name: "Wydane" }).click();
  await expect(manifestCard).toBeHidden();
});

async function refreshPackingUntilReady(page: Page, manifestCard: Locator) {
  for (let attempt = 0; attempt < 20; attempt += 1) {
    if ((await manifestCard.textContent())?.includes("Pakuj")) {
      return;
    }

    await page.getByRole("button", { name: "Odswiez" }).click();
    await page.waitForTimeout(500);
  }

  await expect(manifestCard).toContainText("Pakuj");
}
