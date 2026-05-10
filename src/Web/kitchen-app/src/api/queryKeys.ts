export const queryKeys = {
  stations: ["kitchen", "stations"] as const,
  stationTasks: (stationId: string) => ["kitchen", "stations", stationId, "tasks"] as const
};
