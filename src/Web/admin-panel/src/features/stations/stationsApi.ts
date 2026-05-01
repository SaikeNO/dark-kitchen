import { getJson, postJson, putJson } from "../../api/http";
import type { Station, StationPayload } from "./stationTypes";

export function listStations(signal?: AbortSignal) {
  return getJson<Station[]>("/api/admin/stations", signal);
}

export function saveStation(stationId: string | null, payload: StationPayload) {
  return stationId === null
    ? postJson<Station>("/api/admin/stations", payload)
    : putJson<Station>(`/api/admin/stations/${stationId}`, payload);
}

export function deactivateStation(stationId: string) {
  return postJson<unknown>(`/api/admin/stations/${stationId}/deactivate`);
}
