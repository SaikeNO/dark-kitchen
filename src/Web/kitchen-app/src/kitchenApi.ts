import { getJson, postJson } from "./api/http";
import type { KitchenTask, Station } from "./kitchenTypes";

export function listStations(signal?: AbortSignal) {
  return getJson<Station[]>("/api/kitchen/stations", signal);
}

export function listStationTasks(stationId: string, signal?: AbortSignal) {
  return getJson<KitchenTask[]>(`/api/kitchen/stations/${stationId}/tasks`, signal);
}

export function startTask(taskId: string) {
  return postJson<KitchenTask>(`/api/kitchen/tasks/${taskId}/start`);
}

export function completeTask(taskId: string) {
  return postJson<KitchenTask>(`/api/kitchen/tasks/${taskId}/done`);
}
