# Strategia testów

Repozytorium używa podejścia bliższego testing trophy niż klasycznej piramidzie testów.

## Warstwy

- Testy architektury: szybkie reguły zależności projektów i granic serwisów.
- Testy kontraktowe: stabilność publicznych komunikatów, JSON camelCase i wersjonowanie zdarzeń.
- Testy integracyjne Aspire: realny AppHost, PostgreSQL, RabbitMQ, Redis i procesy API.
- Testy E2E: Playwright uruchamia AppHost z aplikacjami Vite i sprawdza zachowanie z perspektywy przeglądarki.

## Komendy

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

- Testy E2E powinny obejmować tylko przepływy krytyczne z perspektywy użytkownika.
- Szczegółowe przypadki błędów domenowych powinny trafiać najpierw do testów integracyjnych Aspire.
- E2E nie powinny mockować API ani brokera, chyba że test dotyczy jawnie trybu awarii zewnętrznego adaptera.
- Selektory powinny opierać się o role, tekst i stabilne atrybuty użytkowe, nie o klasy CSS.
