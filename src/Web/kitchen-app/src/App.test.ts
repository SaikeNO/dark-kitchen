import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createElement } from "react";
import { renderToString } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { App } from "./App";
import { selectedStationStorageKey, readSelectedStationId, writeSelectedStationId } from "./stationStorage";
import { applyTaskUpdate } from "./taskCache";
import type { KitchenTask } from "./kitchenTypes";

describe("Kitchen App", () => {
  it("renders the shell title", () => {
    const queryClient = new QueryClient();

    expect(renderToString(
      createElement(QueryClientProvider, { client: queryClient }, createElement(App))
    )).toContain("Kitchen App");
  });

  it("stores and clears the selected station", () => {
    const storage = new MemoryStorage();

    writeSelectedStationId("station-1", storage);
    expect(storage.getItem(selectedStationStorageKey)).toBe("station-1");
    expect(readSelectedStationId(storage)).toBe("station-1");

    writeSelectedStationId(null, storage);
    expect(readSelectedStationId(storage)).toBeNull();
  });

  it("updates task cache and removes done tasks", () => {
    const pending = task("task-1", "Pending");
    const active = task("task-1", "InProgress");
    const done = task("task-1", "Done");

    expect(applyTaskUpdate([], pending)).toEqual([pending]);
    expect(applyTaskUpdate([pending], active)).toEqual([active]);
    expect(applyTaskUpdate([active], done)).toEqual([]);
  });
});

function task(id: string, status: KitchenTask["status"]): KitchenTask {
  return {
    id,
    ticketId: "ticket-1",
    orderId: "order-1",
    orderItemId: "item-1",
    menuItemId: "menu-1",
    itemName: "Burger",
    quantity: 1,
    stationId: "station-1",
    stationCode: "GRILL",
    status,
    createdAt: "2026-05-10T10:00:00Z",
    startedAt: null,
    completedAt: status === "Done" ? "2026-05-10T10:01:00Z" : null
  };
}

class MemoryStorage implements Pick<Storage, "getItem" | "removeItem" | "setItem"> {
  private readonly values = new Map<string, string>();

  getItem(key: string) {
    return this.values.get(key) ?? null;
  }

  removeItem(key: string) {
    this.values.delete(key);
  }

  setItem(key: string, value: string) {
    this.values.set(key, value);
  }
}
