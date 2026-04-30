export interface AppMetadata {
  readonly name: string;
  readonly context: string;
  readonly description: string;
}

export interface AppShellProps {
  readonly metadata: AppMetadata;
  readonly apiConfigured: boolean;
}

export function AppShell({ metadata, apiConfigured }: AppShellProps) {
  return (
    <main className="shell">
      <section className="workspace">
        <p className="eyebrow">Dark Kitchen</p>
        <h1>{metadata.name}</h1>
        <p>{metadata.description}</p>
        <dl>
          <div>
            <dt>Kontekst</dt>
            <dd>{metadata.context}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>Foundation ready</dd>
          </div>
          <div>
            <dt>API</dt>
            <dd>{apiConfigured ? "Configured" : "Not configured"}</dd>
          </div>
        </dl>
      </section>
    </main>
  );
}
