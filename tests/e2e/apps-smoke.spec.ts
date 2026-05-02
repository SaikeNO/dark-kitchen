import { expect, test } from "@playwright/test";

const apps = [
  {
    name: "admin-panel",
    url: "http://127.0.0.1:5173",
    heading: "Admin Panel",
    context: undefined
  },
  {
    name: "inventory-panel",
    url: "http://127.0.0.1:5177",
    heading: "Magazyn",
    context: undefined
  },
  {
    name: "storefront",
    url: "http://127.0.0.1:5174/brands/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001",
    heading: "Burger Ghost",
    context: "White-label Storefront"
  },
  {
    name: "kitchen-app",
    url: "http://127.0.0.1:5175",
    heading: "Kitchen App",
    context: "KDS"
  },
  {
    name: "packing-terminal",
    url: "http://127.0.0.1:5176",
    heading: "Packing Terminal",
    context: "Packing"
  }
] as const;

for (const app of apps) {
  test(`${app.name} renders the first screen`, async ({ page }) => {
    const browserErrors: string[] = [];

    page.on("console", message => {
      if (message.type() === "error") {
        const text = message.text();
        if (app.name === "admin-panel" && text.includes("status of 401")) {
          return;
        }

        browserErrors.push(text);
      }
    });

    page.on("pageerror", error => {
      browserErrors.push(error.message);
    });

    await page.goto(app.url);

    await expect(page.getByRole("heading", { name: app.heading })).toBeVisible();
    await expect(page.getByText("Dark Kitchen", { exact: true })).toBeVisible();
    if (app.name === "admin-panel") {
      await expect(page.getByRole("button", { name: "Sign in" })).toBeVisible();
      expect(browserErrors).toEqual([]);
      return;
    }

    if (app.name === "storefront") {
      await expect(page.getByText(app.context, { exact: true })).toBeVisible();
      expect(browserErrors).toEqual([]);
      return;
    }

    if (app.context !== undefined) {
      await expect(page.locator("dd").getByText(app.context, { exact: true })).toBeVisible();
      await expect(page.locator("dd").getByText("Foundation ready", { exact: true })).toBeVisible();
    }

    expect(browserErrors).toEqual([]);
  });
}
