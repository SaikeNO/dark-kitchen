# Dark Kitchen

[![CI](https://github.com/SaikeNO/dark-kitchen/actions/workflows/ci.yml/badge.svg)](https://github.com/SaikeNO/dark-kitchen/actions/workflows/ci.yml)
[![CodeQL](https://github.com/SaikeNO/dark-kitchen/actions/workflows/security.yml/badge.svg)](https://github.com/SaikeNO/dark-kitchen/actions/workflows/security.yml)

Edukacyjny projekt MVP systemu dla wielomarkowej kuchni typu **dark kitchen**. Repozytorium służy do ćwiczenia architektury mikroserwisowej, komunikacji zdarzeniowej, testów integracyjnych z .NET Aspire oraz frontendów operacyjnych dla kuchni, pakowania i sprzedaży.

Projekt jest na wczesnym etapie: obecnie zawiera fundament solution, AppHost Aspire, szkielety usług API, aplikacje React/Vite oraz skonfigurowane warstwy testów. Logika domenowa będzie rozwijana zgodnie z roadmapą w `docs`.

## Co Budujemy

System obsługuje scenariusz dark kitchen, w którym jedna fizyczna kuchnia prowadzi wiele marek jedzeniowych i przyjmuje zamówienia z własnego storefrontu oraz kanałów delivery.

Docelowy przepływ:

```text
Storefront / Mock Delivery
        |
        v
Order Management Service
        |
        v
Inventory Service -> Catalog & Recipe Service
        |
        v
Kitchen Display System
        |
        v
Packing Service
```

Najważniejsze obszary:

- katalog marek, menu, receptur i stacji kuchennych,
- składanie zamówień przez storefront i adaptery delivery,
- koordynacja zamówienia przez OMS i sagę,
- rezerwacja składników w inventory,
- obsługa przygotowania dań w KDS,
- kompletowanie zamówienia na wydawce,
- komunikacja zdarzeniowa przez RabbitMQ.

## Stos Technologiczny

Backend:

- .NET 10, C#, ASP.NET Core Web API
- .NET Aspire AppHost i ServiceDefaults
- PostgreSQL jako baza per service
- RabbitMQ jako broker wiadomości
- Redis dla krótkotrwałego stanu i przyszłych scenariuszy realtime

Frontend:

- React 19
- TypeScript
- Vite
- osobne aplikacje dla storefrontu, panelu admina, kuchni i wydawki

Testy:

- xUnit
- Aspire.Hosting.Testing
- Playwright
- Vitest
- testy architektury i kontraktów

## Struktura Repozytorium

```text
.
|-- docs/                         # ADR-y, opis usług, roadmapa i standardy
|-- src/
|   |-- DarkKitchen.AppHost/       # Orkiestracja lokalna przez .NET Aspire
|   |-- DarkKitchen.ServiceDefaults/
|   |-- Services/                  # Mikroserwisy API
|   |-- Shared/                    # Wspólne kontrakty
|   `-- Web/                       # Aplikacje React/Vite
|-- tests/
|   |-- DarkKitchen.ArchitectureTests/
|   |-- DarkKitchen.ContractTests/
|   |-- DarkKitchen.IntegrationTests/
|   `-- e2e/                       # Testy Playwright
|-- DarkKitchen.slnx
|-- package.json
`-- playwright.config.ts
```

## Usługi i Aplikacje

Backend API:

- `DarkKitchen.Catalog.Api` - marki, menu, receptury i routing do stacji kuchennych.
- `DarkKitchen.Inventory.Api` - stany magazynowe i rezerwacje składników.
- `DarkKitchen.OrderManagement.Api` - przyjmowanie zamówień, stan zamówienia i saga.
- `DarkKitchen.Storefront.Api` - BFF dla własnego sklepu.
- `DarkKitchen.Kds.Api` - backend dla kitchen display system.
- `DarkKitchen.Packing.Api` - kompletowanie zamówień i wydawka.

Frontend:

- `admin-panel` - panel administracyjny katalogu.
- `storefront` - sklep dla klienta.
- `kitchen-app` - tablet kuchenny dla stanowisk.
- `packing-terminal` - terminal wydawki.

## Wymagania

- .NET SDK 10
- Node.js 24+
- npm 11+
- Docker Desktop albo inne środowisko kontenerów kompatybilne z Aspire
- przeglądarka Chromium instalowana przez Playwright dla testów E2E

## Uruchomienie Lokalne

Zainstaluj zależności:

```powershell
dotnet restore DarkKitchen.slnx
npm install
```

Uruchom cały system przez Aspire:

```powershell
dotnet run --project src/DarkKitchen.AppHost
```

AppHost uruchamia:

- mikroserwisy API,
- PostgreSQL,
- RabbitMQ z management pluginem,
- Redis,
- aplikacje frontendowe Vite,
- Aspire Dashboard.

Po starcie adres dashboardu pojawi się w konsoli.

## Przydatne Komendy

Frontend:

```powershell
npm run dev:admin
npm run dev:storefront
npm run dev:kitchen
npm run dev:packing
```

Build:

```powershell
dotnet build DarkKitchen.slnx
npm run build
```

Lint:

```powershell
npm run lint
```

## Testy

Repozytorium jest ustawione pod podejście bliższe **testing trophy** niż klasycznej piramidzie. Największy nacisk jest położony na testy integracyjne z Aspire, bo projekt ma uczyć pracy z prawdziwymi zależnościami: bazą, brokerem, procesami usług i frontendami.

Cały zestaw testów:

```powershell
npm run test:all
```

Testy .NET:

```powershell
dotnet test DarkKitchen.slnx
```

Wybrane warstwy:

```powershell
npm run test:architecture
npm run test:contracts
npm run test:integration
npm run test:frontend
```

Testy E2E:

```powershell
npm run test:e2e:install
npm run test:e2e
```

Playwright uruchamia AppHost w trybie E2E, z aplikacjami Vite na stałych portach:

- Admin Panel: `http://127.0.0.1:5173`
- Storefront: `http://127.0.0.1:5174`
- Kitchen App: `http://127.0.0.1:5175`
- Packing Terminal: `http://127.0.0.1:5176`

Raport E2E:

```powershell
npm run test:e2e:report
```

## Dokumentacja

Najważniejsze dokumenty:

- [Model biznesowy dark kitchen](./docs/01-business-model-dark-kitchen.md)
- [Architektura](./docs/02-architektura.md)
- [Roadmapa zadań](./docs/03-roadmap-zadan.md)
- [Standardy projektowe i architektoniczne](./docs/04-standardy-projektowe-i-architektoniczne.md)
- [Strategia testów](./docs/05-strategia-testow.md)
- [ADR 011: Stos technologiczny](./docs/adr/ADR-011-stos-technologiczny.md)

Opisy usług:

- [Catalog & Recipe Service](./docs/services/catalog-and-recipe-service.md)
- [Inventory Service](./docs/services/inventory-service.md)
- [Order Management Service](./docs/services/order-managment-service.md)
- [Storefront Service](./docs/services/store-front-service.md)
- [Kitchen Display System Service](./docs/services/kitchen-display-system-service.md)
- [Packing Service](./docs/services/packing-service.md)

## Status Projektu

Gotowe fundamenty:

- solution `.slnx`,
- centralne zarządzanie pakietami NuGet,
- AppHost Aspire,
- ServiceDefaults z OpenTelemetry i health checks,
- szkielety sześciu usług API,
- cztery aplikacje React/Vite,
- projekt wspólnych kontraktów zdarzeń,
- testy architektury, kontraktowe, integracyjne i E2E.

Najbliższy etap:

- implementacja kontraktów i komunikacji zdarzeniowej,
- konfiguracja WolverineFx,
- durable inbox/outbox,
- pierwsze scenariusze domenowe dla zamówień i rezerwacji inventory.

## Charakter Projektu

To repozytorium ma charakter edukacyjny. Priorytetem jest czytelna architektura, dobre granice usług, testowalność i lokalne doświadczenie developerskie, a nie szybkie dostarczenie produkcyjnego systemu gastronomicznego.
