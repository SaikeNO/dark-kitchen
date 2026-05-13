import type { PackingManifest } from "./packingTypes";

export function applyManifestUpdate(
  current: readonly PackingManifest[],
  manifest: PackingManifest)
{
  const next = current.filter(item => item.id !== manifest.id);
  if (manifest.status !== "Issued") {
    next.push(manifest);
  }

  return sortManifests(next);
}

export function groupManifests(manifests: readonly PackingManifest[]) {
  return {
    waiting: manifests.filter(manifest => manifest.status === "Waiting"),
    ready: manifests.filter(manifest => manifest.status === "ReadyForPacking"),
    delayed: manifests.filter(manifest => manifest.status === "Delayed" || manifest.isDelayed)
  };
}

function sortManifests(manifests: readonly PackingManifest[]) {
  return [...manifests].sort(compareManifests);
}

function compareManifests(left: PackingManifest, right: PackingManifest) {
  const rankDiff = statusRank(left) - statusRank(right);
  if (rankDiff !== 0) {
    return rankDiff;
  }

  return left.createdAt.localeCompare(right.createdAt);
}

function statusRank(manifest: PackingManifest) {
  if (manifest.status === "Delayed" || manifest.isDelayed) {
    return 0;
  }

  if (manifest.status === "ReadyForPacking") {
    return 1;
  }

  return 2;
}
