import { useMutation, useQuery, useQueryClient, type QueryClient } from "@tanstack/react-query";
import * as signalR from "@microsoft/signalr";
import {
  AlertTriangle,
  Check,
  CheckCircle2,
  Clock3,
  Loader2,
  PackageCheck,
  PlugZap,
  RefreshCw,
  WifiOff
} from "lucide-react";
import { useEffect, useMemo, useState, type ReactNode } from "react";
import { apiConfigured, errorMessage, hubUrl } from "./api/http";
import { queryKeys } from "./api/queryKeys";
import { applyManifestUpdate, groupManifests } from "./manifestCache";
import { issueManifest, listManifests } from "./packingApi";
import type { ConnectionStatus, PackingManifest } from "./packingTypes";
import "./styles.css";

const emptyManifests: PackingManifest[] = [];

export function App() {
  const queryClient = useQueryClient();
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>("offline");

  const manifestsQuery = useQuery({
    queryKey: queryKeys.manifests,
    queryFn: ({ signal }) => listManifests(signal),
    staleTime: 5_000
  });

  usePackingHub(queryClient, setConnectionStatus);

  const manifests = manifestsQuery.data ?? emptyManifests;
  const groups = useMemo(() => groupManifests(manifests), [manifests]);

  const issueMutation = useMutation({
    mutationFn: issueManifest,
    onSuccess: manifest => {
      queryClient.setQueryData<PackingManifest[]>(
        queryKeys.manifests,
        current => applyManifestUpdate(current ?? emptyManifests, manifest));
    }
  });

  function refresh() {
    void queryClient.invalidateQueries({ queryKey: queryKeys.manifests });
  }

  return (
    <main className="packing-shell">
      <header className="packing-header">
        <div>
          <p className="eyebrow">Dark Kitchen</p>
          <h1>Packing Terminal</h1>
        </div>
        <div className="header-actions">
          <ConnectionBadge status={connectionStatus} />
          <button className="icon-button" type="button" aria-label="Odswiez" onClick={refresh}>
            {manifestsQuery.isFetching ? <Loader2 aria-hidden="true" className="spin" /> : <RefreshCw aria-hidden="true" />}
          </button>
        </div>
      </header>

      {manifestsQuery.isError && (
        <p className="error-banner">{errorMessage(manifestsQuery.error, "Nie udalo sie pobrac manifestow.")}</p>
      )}

      {manifestsQuery.isPending ? (
        <StateBlock icon={<Loader2 className="spin" aria-hidden="true" />} text="Ladowanie manifestow" />
      ) : manifests.length === 0 ? (
        <StateBlock icon={<PackageCheck aria-hidden="true" />} text="Brak zamowien na wydawce" />
      ) : (
        <section className="manifest-board">
          <ManifestColumn
            title={`Alerty (${groups.delayed.length})`}
            icon={<AlertTriangle aria-hidden="true" />}
            manifests={groups.delayed}
            emptyText="Brak opoznionych manifestow"
            issuePending={issueMutation.isPending}
            onIssue={manifest => issueMutation.mutate(manifest.id)}
          />
          <ManifestColumn
            title={`Gotowe (${groups.ready.length})`}
            icon={<CheckCircle2 aria-hidden="true" />}
            manifests={groups.ready}
            emptyText="Nic nie czeka na wydanie"
            issuePending={issueMutation.isPending}
            onIssue={manifest => issueMutation.mutate(manifest.id)}
          />
          <ManifestColumn
            title={`Kompletacja (${groups.waiting.length})`}
            icon={<Clock3 aria-hidden="true" />}
            manifests={groups.waiting}
            emptyText="Brak manifestow w toku"
            issuePending={issueMutation.isPending}
            onIssue={manifest => issueMutation.mutate(manifest.id)}
          />
        </section>
      )}

      <footer className="packing-footer">API: {apiConfigured ? "configured" : "relative URL"}</footer>
    </main>
  );
}

function ManifestColumn({
  emptyText,
  icon,
  issuePending,
  manifests,
  onIssue,
  title
}: {
  readonly emptyText: string;
  readonly icon: ReactNode;
  readonly issuePending: boolean;
  readonly manifests: readonly PackingManifest[];
  readonly onIssue: (manifest: PackingManifest) => void;
  readonly title: string;
}) {
  return (
    <section className="manifest-column">
      <div className="column-title">
        {icon}
        <h2>{title}</h2>
      </div>
      {manifests.length === 0 ? (
        <p className="empty-column">{emptyText}</p>
      ) : (
        <div className="manifest-list">
          {manifests.map(manifest => (
            <ManifestCard
              key={manifest.id}
              manifest={manifest}
              issuePending={issuePending}
              onIssue={onIssue}
            />
          ))}
        </div>
      )}
    </section>
  );
}

function ManifestCard({
  issuePending,
  manifest,
  onIssue
}: {
  readonly issuePending: boolean;
  readonly manifest: PackingManifest;
  readonly onIssue: (manifest: PackingManifest) => void;
}) {
  const canIssue = manifest.status === "ReadyForPacking";
  const progress = `${manifest.readyItemsCount}/${manifest.totalItemsCount}`;

  return (
    <article className={`manifest-card ${manifest.isDelayed ? "delayed" : ""}`}>
      <div className="manifest-topline">
        <div>
          <p className="order-ref">Order {shortId(manifest.orderId)}</p>
          <h3>{progress} gotowe</h3>
        </div>
        <StatusBadge manifest={manifest} />
      </div>

      <div className="progress-track" aria-label={`Kompletacja ${progress}`}>
        <span style={{ width: `${completionPercent(manifest)}%` }} />
      </div>

      <ul className="item-list">
        {manifest.items.map(item => (
          <li key={item.id} className={item.isReady ? "ready" : "waiting"}>
            <span>{item.itemName}</span>
            <strong>{item.quantity}x</strong>
          </li>
        ))}
      </ul>

      <button
        className="issue-button"
        type="button"
        disabled={!canIssue || issuePending}
        onClick={() => onIssue(manifest)}
      >
        <Check aria-hidden="true" />
        <span>Wydane</span>
      </button>
    </article>
  );
}

function StatusBadge({ manifest }: { readonly manifest: PackingManifest }) {
  const label = manifest.isDelayed
    ? "Alert"
    : manifest.status === "ReadyForPacking"
      ? "Pakuj"
      : "Czeka";

  return <span className={`status-badge ${manifest.isDelayed ? "delayed" : manifest.status}`}>{label}</span>;
}

function ConnectionBadge({ status }: { readonly status: ConnectionStatus }) {
  const label = status === "connected" ? "Online" : status === "connecting" ? "Laczenie" : "Offline";
  return (
    <span className={`connection-badge ${status}`}>
      {status === "connected" ? <PlugZap aria-hidden="true" /> : <WifiOff aria-hidden="true" />}
      {label}
    </span>
  );
}

function StateBlock({ icon, text }: { readonly icon: ReactNode; readonly text: string }) {
  return (
    <div className="state-block">
      {icon}
      <p>{text}</p>
    </div>
  );
}

function usePackingHub(
  queryClient: QueryClient,
  setConnectionStatus: (status: ConnectionStatus) => void)
{
  useEffect(() => {
    let disposed = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.on("manifestChanged", (manifest: PackingManifest) => {
      queryClient.setQueryData<PackingManifest[]>(
        queryKeys.manifests,
        current => applyManifestUpdate(current ?? emptyManifests, manifest));
    });

    connection.onreconnecting(() => setConnectionStatus("connecting"));
    connection.onreconnected(() => {
      setConnectionStatus("connected");
      void queryClient.invalidateQueries({ queryKey: queryKeys.manifests });
    });
    connection.onclose(() => {
      if (!disposed) {
        setConnectionStatus("offline");
      }
    });

    async function start() {
      setConnectionStatus("connecting");
      try {
        await connection.start();
        if (!disposed) {
          setConnectionStatus("connected");
        }
      } catch {
        if (!disposed) {
          setConnectionStatus("offline");
        }
      }
    }

    void start();

    return () => {
      disposed = true;
      setConnectionStatus("offline");
      void connection.stop();
    };
  }, [queryClient, setConnectionStatus]);
}

function completionPercent(manifest: PackingManifest) {
  if (manifest.totalItemsCount === 0) {
    return 0;
  }

  return Math.round((manifest.readyItemsCount / manifest.totalItemsCount) * 100);
}

function shortId(value: string) {
  return value.slice(0, 8);
}
