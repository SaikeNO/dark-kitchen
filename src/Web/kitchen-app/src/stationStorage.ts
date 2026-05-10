export const selectedStationStorageKey = "dark-kitchen-kds-station-id";

export function readSelectedStationId(storage: Pick<Storage, "getItem">) {
  const value = storage.getItem(selectedStationStorageKey);
  return value === null || value.trim().length === 0 ? null : value;
}

export function writeSelectedStationId(stationId: string | null, storage: Pick<Storage, "removeItem" | "setItem"> = window.localStorage) {
  if (stationId === null) {
    storage.removeItem(selectedStationStorageKey);
    return;
  }

  storage.setItem(selectedStationStorageKey, stationId);
}
