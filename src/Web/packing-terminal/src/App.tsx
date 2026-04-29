import { appMetadata } from "./appMetadata";
import "./styles.css";

export function App() {
  return (
    <main className="shell">
      <section className="workspace">
        <p className="eyebrow">Dark Kitchen</p>
        <h1>{appMetadata.name}</h1>
        <p>{appMetadata.description}</p>
        <dl>
          <div>
            <dt>Kontekst</dt>
            <dd>{appMetadata.context}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>Foundation ready</dd>
          </div>
        </dl>
      </section>
    </main>
  );
}

