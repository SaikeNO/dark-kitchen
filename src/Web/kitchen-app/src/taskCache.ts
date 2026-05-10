import type { KitchenTask } from "./kitchenTypes";

export function applyTaskUpdate(current: readonly KitchenTask[], task: KitchenTask) {
  const next = current.filter(item => item.id !== task.id);
  if (task.status !== "Done") {
    next.push(task);
  }

  return next.sort(compareTasks);
}

function compareTasks(left: KitchenTask, right: KitchenTask) {
  if (left.status !== right.status) {
    return statusRank(left.status) - statusRank(right.status);
  }

  return left.createdAt.localeCompare(right.createdAt);
}

function statusRank(status: KitchenTask["status"]) {
  return status === "InProgress" ? 0 : 1;
}
