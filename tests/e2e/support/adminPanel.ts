import { expect, test as base, type Page } from "@playwright/test";

export const ADMIN_URL = "http://127.0.0.1:5173";

export const test = base.extend<{ browserErrors: string[] }>({
  browserErrors: [
    async ({ page }, use) => {
      const browserErrors: string[] = [];

      page.on("console", message => {
        if (message.type() !== "error") {
          return;
        }

        const text = message.text();
        if (text.includes("status of 401")) {
          return;
        }

        browserErrors.push(text);
      });

      page.on("pageerror", error => {
        browserErrors.push(error.message);
      });

      await use(browserErrors);

      expect(browserErrors).toEqual([]);
    },
    { auto: true }
  ]
});

export { expect };

export function uniqueName(prefix: string) {
  const suffix = `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`;
  return `E2E ${prefix} ${suffix}`;
}

export async function openAdminPanel(page: Page) {
  await expect(async () => {
    await page.goto(ADMIN_URL, { timeout: 5_000, waitUntil: "domcontentloaded" });
    await expect(page.getByRole("heading", { level: 2, name: "Sign in" })).toBeVisible({ timeout: 2_000 });
  }).toPass({ timeout: 60_000 });
}

export async function loginAsManager(page: Page) {
  await loginWithDemoAccount(page, "Manager demo", "manager@darkkitchen.local", "Manager");
}

export async function loginAsOperator(page: Page) {
  await loginWithDemoAccount(page, "Operator demo", "operator@darkkitchen.local", "Operator");
}

export function cardByText(page: Page, text: string) {
  return page.getByRole("article").filter({ hasText: text });
}

export function mainHeading(page: Page, name: string) {
  return page.locator("#main-content").getByRole("heading", { name });
}

async function loginWithDemoAccount(page: Page, accountButton: string, email: string, role: string) {
  await openAdminPanel(page);
  await page.getByRole("button", { name: accountButton }).click();
  const loginResponse = page.waitForResponse(response => response.url().includes("/api/admin/auth/login"));
  await page.getByRole("button", { name: "Sign in" }).click();

  expect((await loginResponse).status()).toBe(200);
  await expect(page.getByText(email, { exact: true })).toBeVisible();
  await expect(page.getByText(role, { exact: true })).toBeVisible();
  await expect(mainHeading(page, "Marki")).toBeVisible();
}
