import { useMutation, useQuery, useQueryClient, type QueryClient } from "@tanstack/react-query";
import * as signalR from "@microsoft/signalr";
import {
  ArrowLeft,
  Check,
  CheckCircle2,
  ChefHat,
  Clock3,
  Loader2,
  Play,
  PlugZap,
  RefreshCw,
  WifiOff
} from "lucide-react";
import { useEffect, useMemo, useState, type ReactNode } from "react";
import { apiConfigured, errorMessage, hubUrl } from "./api/http";
import { queryKeys } from "./api/queryKeys";
import { completeTask, listStationTasks, listStations, startTask } from "./kitchenApi";
import type { ConnectionStatus, KitchenTask, Station } from "./kitchenTypes";
import { applyTaskUpdate } from "./taskCache";
import { readSelectedStationId, writeSelectedStationId } from "./stationStorage";
import "./styles.css";

const emptyStations: Station[] = [];
const emptyTasks: KitchenTask[] = [];

export function App() {
  const queryClient = useQueryClient();
  const [selectedStationId, setSelectedStationId] = useState(readInitialStationId);
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>("offline");

  const stationsQuery = useQuery({
    queryKey: queryKeys.stations,
    queryFn: ({ signal }) => listStations(signal),
    staleTime: 30_000
  });

  const stations = stationsQuery.data ?? emptyStations;
  const selectedStation = stations.find(station => station.id === selectedStationId) ?? null;

  useEffect(() => {
    if (stationsQuery.isSuccess && selectedStationId !== null && selectedStation === null) {
      writeSelectedStationId(null);
    }
  }, [selectedStation, selectedStationId, stationsQuery.isSuccess]);

  const tasksQuery = useQuery({
    queryKey: queryKeys.stationTasks(selectedStationId ?? ""),
    queryFn: ({ signal }) => listStationTasks(selectedStationId ?? "", signal),
    enabled: selectedStation !== null,
    staleTime: 5_000
  });

  useKitchenHub(selectedStation?.id ?? null, queryClient, setConnectionStatus);

  const visibleTasks = tasksQuery.data ?? emptyTasks;
  const pendingTasks = useMemo(
    () => visibleTasks.filter(task => task.status === "Pending"),
    [visibleTasks]
  );
  const activeTasks = useMemo(
    () => visibleTasks.filter(task => task.status === "InProgress"),
    [visibleTasks]
  );

  const taskMutation = useMutation({
    mutationFn: (request: { readonly action: () => Promise<KitchenTask> }) => request.action(),
    onSuccess: task => {
      if (selectedStationId === null) {
        return;
      }

      queryClient.setQueryData<KitchenTask[]>(
        queryKeys.stationTasks(selectedStationId),
        current => applyTaskUpdate(current ?? emptyTasks, task));
    }
  });

  function selectStation(station: Station) {
    setSelectedStationId(station.id);
    writeSelectedStationId(station.id);
  }

  function clearStation() {
    setSelectedStationId(null);
    writeSelectedStationId(null);
    setConnectionStatus("offline");
  }

  function refresh() {
    void queryClient.invalidateQueries({ queryKey: selectedStation === null ? queryKeys.stations : queryKeys.stationTasks(selectedStation.id) });
  }

  return (
    <main className="kds-shell">
      <header className="kds-header">
        <div>
          <p className="eyebrow">Dark Kitchen</p>
          <h1>Kitchen App</h1>
        </div>
        <div className="header-actions">
          {selectedStation !== null && (
            <button className="icon-button" type="button" aria-label="Zmien stacje" onClick={clearStation}>
              <ArrowLeft aria-hidden="true" />
            </button>
          )}
          <button className="icon-button" type="button" aria-label="Odswiez" onClick={refresh}>
            {tasksQuery.isFetching || stationsQuery.isFetching ? <Loader2 aria-hidden="true" className="spin" /> : <RefreshCw aria-hidden="true" />}
          </button>
        </div>
      </header>

      {selectedStation === null ? (
        <StationPicker
          isLoading={stationsQuery.isPending}
          stations={stations}
          onSelect={selectStation}
        />
      ) : (
        <KitchenBoard
          activeTasks={activeTasks}
          connectionStatus={connectionStatus}
          error={tasksQuery.isError ? errorMessage(tasksQuery.error, "Nie udalo sie pobrac zadan.") : null}
          isLoading={tasksQuery.isPending}
          mutationPending={taskMutation.isPending}
          pendingTasks={pendingTasks}
          selectedStation={selectedStation}
          startTask={task => taskMutation.mutate({ action: () => startTask(task.id) })}
          completeTask={task => taskMutation.mutate({ action: () => completeTask(task.id) })}
        />
      )}

      <footer className="kds-footer">API: {apiConfigured ? "configured" : "relative URL"}</footer>
    </main>
  );
}

function StationPicker({
  isLoading,
  stations,
  onSelect
}: {
  readonly isLoading: boolean;
  readonly stations: Station[];
  readonly onSelect: (station: Station) => void;
}) {
  return (
    <section className="station-picker">
      <div className="section-title">
        <ChefHat aria-hidden="true" />
        <h2>Wybierz stacje</h2>
      </div>

      {isLoading ? (
        <StateBlock icon={<Loader2 className="spin" aria-hidden="true" />} text="Ladowanie stacji" />
      ) : stations.length === 0 ? (
        <StateBlock icon={<WifiOff aria-hidden="true" />} text="Brak aktywnych stacji" />
      ) : (
        <div className="station-grid">
          {stations.map(station => (
            <button
              className="station-card"
              key={station.id}
              type="button"
              style={{ borderColor: station.displayColor }}
              onClick={() => onSelect(station)}
            >
              <span className="station-code" style={{ backgroundColor: station.displayColor }}>{station.code}</span>
              <strong>{station.name}</strong>
            </button>
          ))}
        </div>
      )}
    </section>
  );
}

function KitchenBoard({
  activeTasks,
  completeTask,
  connectionStatus,
  error,
  isLoading,
  mutationPending,
  pendingTasks,
  selectedStation,
  startTask
}: {
  readonly activeTasks: KitchenTask[];
  readonly completeTask: (task: KitchenTask) => void;
  readonly connectionStatus: ConnectionStatus;
  readonly error: string | null;
  readonly isLoading: boolean;
  readonly mutationPending: boolean;
  readonly pendingTasks: KitchenTask[];
  readonly selectedStation: Station;
  readonly startTask: (task: KitchenTask) => void;
}) {
  return (
    <section className="board">
      <div className="station-bar" style={{ borderColor: selectedStation.displayColor }}>
        <div>
          <span className="station-code" style={{ backgroundColor: selectedStation.displayColor }}>{selectedStation.code}</span>
          <h2>{selectedStation.name}</h2>
        </div>
        <ConnectionBadge status={connectionStatus} />
      </div>

      {error !== null && <p className="error-banner">{error}</p>}

      {isLoading ? (
        <StateBlock icon={<Loader2 className="spin" aria-hidden="true" />} text="Ladowanie zadan" />
      ) : (
        <div className="task-columns">
          <TaskColumn
            emptyText="Brak zadan w kolejce"
            icon={<Clock3 aria-hidden="true" />}
            title={`Kolejka (${pendingTasks.length})`}
            tasks={pendingTasks}
            actionLabel="Start"
            actionIcon={<Play aria-hidden="true" />}
            disabled={mutationPending}
            onAction={startTask}
          />
          <TaskColumn
            emptyText="Nic nie jest w trakcie"
            icon={<CheckCircle2 aria-hidden="true" />}
            title={`W trakcie (${activeTasks.length})`}
            tasks={activeTasks}
            actionLabel="Done"
            actionIcon={<Check aria-hidden="true" />}
            disabled={mutationPending}
            onAction={completeTask}
          />
        </div>
      )}
    </section>
  );
}

function TaskColumn({
  actionIcon,
  actionLabel,
  disabled,
  emptyText,
  icon,
  onAction,
  tasks,
  title
}: {
  readonly actionIcon: ReactNode;
  readonly actionLabel: string;
  readonly disabled: boolean;
  readonly emptyText: string;
  readonly icon: ReactNode;
  readonly onAction: (task: KitchenTask) => void;
  readonly tasks: KitchenTask[];
  readonly title: string;
}) {
  return (
    <section className="task-column">
      <div className="column-title">
        {icon}
        <h3>{title}</h3>
      </div>
      {tasks.length === 0 ? (
        <p className="empty-column">{emptyText}</p>
      ) : (
        <div className="task-list">
          {tasks.map(task => (
            <article className="task-card" key={task.id}>
              <div>
                <p className="order-ref">Order {shortId(task.orderId)}</p>
                <h4>{task.itemName}</h4>
                <p className="task-meta">Qty {task.quantity} - item {shortId(task.orderItemId)}</p>
              </div>
              <button
                className="task-action"
                type="button"
                disabled={disabled}
                onClick={() => onAction(task)}
              >
                {actionIcon}
                <span>{actionLabel}</span>
              </button>
            </article>
          ))}
        </div>
      )}
    </section>
  );
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

function useKitchenHub(
  stationId: string | null,
  queryClient: QueryClient,
  setConnectionStatus: (status: ConnectionStatus) => void)
{
  useEffect(() => {
    if (stationId === null) {
      return;
    }

    let disposed = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.on("taskChanged", (task: KitchenTask) => {
      queryClient.setQueryData<KitchenTask[]>(
        queryKeys.stationTasks(stationId),
        current => applyTaskUpdate(current ?? emptyTasks, task));
    });

    connection.onreconnecting(() => setConnectionStatus("connecting"));
    connection.onreconnected(() => {
      setConnectionStatus("connected");
      void connection.invoke("JoinStation", stationId);
      void queryClient.invalidateQueries({ queryKey: queryKeys.stationTasks(stationId) });
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
        await connection.invoke("JoinStation", stationId);
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
  }, [queryClient, setConnectionStatus, stationId]);
}

function readInitialStationId() {
  if (typeof window === "undefined") {
    return null;
  }

  return readSelectedStationId(window.localStorage);
}

function shortId(value: string) {
  return value.slice(0, 8);
}
