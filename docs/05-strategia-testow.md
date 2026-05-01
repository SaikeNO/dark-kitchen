# Strategia testów

Repozytorium używa podejścia bliższego testing trophy niż klasycznej piramidzie testów. Największy nacisk kładziemy na testy integracyjne backendu, bo endpointy mają działać z prawdziwym AppHostem Aspire, PostgreSQL, RabbitMQ, Redis i procesami API.

## Warstwy

- Testy architektury: szybkie reguły zależności projektów, granic serwisów i wymogu testu integracyjnego dla każdego `*Endpoint.cs`.
- Testy kontraktowe: stabilność zdarzeń integracyjnych, `eventType`, wersjonowanie, envelope i publiczny JSON camelCase.
- Testy integracyjne per serwis: osobne projekty `DarkKitchen.<Service>.IntegrationTests`, uruchamiane przez wspólny fixture AppHost.
- Testy jednostkowe domeny: tylko dla nietrywialnych encji/agregatów z realnymi inwariantami, kalkulacją, stanami lub idempotencją.
- Testy E2E: Playwright sprawdza krytyczne ścieżki z perspektywy użytkownika.

## Struktura

```text
tests/
|-- Architecture/DarkKitchen.ArchitectureTests/
|-- Contracts/DarkKitchen.ContractTests/
|-- Integration/
|   |-- AppHost/DarkKitchen.AppHost.IntegrationTests/
|   |-- Catalog/DarkKitchen.Catalog.IntegrationTests/
|   |-- Inventory/DarkKitchen.Inventory.IntegrationTests/
|   |-- Kds/DarkKitchen.Kds.IntegrationTests/
|   |-- OrderManagement/DarkKitchen.OrderManagement.IntegrationTests/
|   |-- Packing/DarkKitchen.Packing.IntegrationTests/
|   `-- Storefront/DarkKitchen.Storefront.IntegrationTests/
|-- Shared/DarkKitchen.Testing/
`-- e2e/
```

`DarkKitchen.Testing` zawiera wspólny `AspireAppFixture`, helpery HTTP/JSON, logowanie testowe Catalog i buildery danych dla endpointów. Fixture serializuje start AppHosta między procesami testowymi, dzięki czemu `dotnet test DarkKitchen.slnx` może przejść nawet wtedy, gdy CLI zaplanuje projekty integracyjne równolegle.

## Komendy

```powershell
npm run test:architecture
npm run test:contracts
npm run test:integration
dotnet test DarkKitchen.slnx
```

`npm run test:integration` uruchamia projekty integracyjne sekwencyjnie. To jest stabilny tryb dla Aspire, bo każdy projekt startuje pełny AppHost z:

- `DarkKitchen:IncludeWebApps=false`
- `DarkKitchen:UsePersistentVolumes=false`

Testy E2E:

```powershell
npm run test:e2e:install
npm run test:e2e
npm run test:e2e:headed
npm run test:e2e:report
```

`npm run test:e2e` buduje AppHost, startuje go przez Playwright i przekazuje konfigurację:

- `DarkKitchen:UsePersistentVolumes=false`
- `DarkKitchen:UseFixedWebPorts=true`

Stałe porty E2E:

- Admin Panel: `http://127.0.0.1:5173`
- Storefront: `http://127.0.0.1:5174`
- Kitchen App: `http://127.0.0.1:5175`
- Packing Terminal: `http://127.0.0.1:5176`

## Zasady

- Każdy endpoint handler `XEndpoint.cs` musi mieć test integracyjny `XEndpointTests.cs` w projekcie integracyjnym danego serwisu.
- Testy integracyjne traktują API jako black box: używają HTTP i własnych DTO testowych, bez referencji do `Features` lub `Infrastructure`.
- Minimalne pokrycie endpointu to happy path, autoryzacja/polityka roli, walidacja wejścia lub błąd biznesowy oraz efekt trwały widoczny przez publiczne HTTP.
- Contract tests dotyczą zdarzeń, nie snapshotów HTTP.
- Szczegółowe przypadki błędów domenowych trafiają najpierw do testów integracyjnych Aspire.
- E2E obejmują tylko przepływy krytyczne z perspektywy użytkownika.
- E2E nie powinny mockować API ani brokera, chyba że test dotyczy jawnie trybu awarii zewnętrznego adaptera.
- Selektory UI powinny opierać się o role, tekst i stabilne atrybuty użytkowe, nie o klasy CSS.
