# Dark Kitchen

Edukacyjne MVP systemu do zarządzania wielomarkową kuchnią typu dark kitchen. Architektura i roadmapa są opisane w [docs](./docs).

## Uruchomienie lokalne

Wymagania:

- .NET SDK 10
- Node.js 24+
- Docker lub zgodne środowisko kontenerów dla zasobów Aspire

Pierwsze uruchomienie:

```powershell
dotnet restore
npm install
dotnet run --project src/DarkKitchen.AppHost
```

AppHost uruchamia mikroserwisy API, PostgreSQL, RabbitMQ, Redis oraz aplikacje frontendowe Vite.

## Testy

Repozytorium jest ustawione pod testy w stylu "testing trophy": niewiele unit testów dla czystej logiki, szybkie testy architektury i kontraktów oraz grubsza warstwa testów integracyjnych na prawdziwym AppHost Aspire.

Najczęściej używane komendy:

```powershell
dotnet test DarkKitchen.slnx
npm run test:architecture
npm run test:contracts
npm run test:integration
npm run test:e2e:install
npm run test:e2e
npm run test:frontend
npm run test:all
```

Testy integracyjne używają `Aspire.Hosting.Testing`, uruchamiają realne zasoby PostgreSQL, RabbitMQ i Redis, ale przekazują do AppHosta `DarkKitchen:IncludeWebApps=false` oraz `DarkKitchen:UsePersistentVolumes=false`. Dzięki temu nie startują Vite i nie zostaje stan w wolumenach po testach.

Testy E2E używają Playwright i uruchamiają AppHost z aplikacjami Vite na stałych portach. Szczegóły są w [strategii testów](./docs/05-strategia-testow.md).

## Ważne dokumenty

- [ADR 011: stos technologiczny](./docs/adr/ADR-011-stos-technologiczny.md)
- [Roadmap zadań](./docs/03-roadmap-zadan.md)
- [Standardy projektowe i architektoniczne](./docs/04-standardy-projektowe-i-architektoniczne.md)
- [Strategia testów](./docs/05-strategia-testow.md)
