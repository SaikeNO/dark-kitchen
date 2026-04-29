import { defineConfig, devices } from "@playwright/test";

const webServerTimeout = process.env.CI ? 300_000 : 180_000;

export default defineConfig({
  testDir: "./tests/e2e",
  timeout: 60_000,
  expect: {
    timeout: 10_000
  },
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  reporter: [
    ["list"],
    ["html", { outputFolder: "artifacts/playwright-report", open: "never" }]
  ],
  outputDir: "artifacts/playwright-results",
  use: {
    baseURL: "http://127.0.0.1:5174",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "retain-on-failure"
  },
  webServer: {
    command:
      "dotnet run --project src/DarkKitchen.AppHost/DarkKitchen.AppHost.csproj --no-build --launch-profile DarkKitchen.AppHost -- DarkKitchen:UsePersistentVolumes=false DarkKitchen:UseFixedWebPorts=true",
    url: "http://127.0.0.1:5174",
    reuseExistingServer: !process.env.CI,
    timeout: webServerTimeout,
    stdout: "pipe",
    stderr: "pipe"
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] }
    }
  ]
});
