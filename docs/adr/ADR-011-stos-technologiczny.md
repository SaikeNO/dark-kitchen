# ADR 011: Wybór stosu technologicznego dla edukacyjnego MVP Dark Kitchen

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Projekt jest systemem do zarządzania wielomarkową kuchnią typu dark kitchen. Istniejące decyzje architektoniczne przesądzają, że system ma być oparty o mikroserwisy, komunikację zdarzeniową, wzorzec Saga, osobne bazy danych dla usług oraz lokalną orkiestrację przez .NET Aspire.

Pierwsza wersja ma charakter edukacyjnego MVP. Priorytetem jest zrozumiały, spójny i możliwy do uruchomienia lokalnie system, który pokazuje pełny przepływ zamówienia: kanał sprzedaży lub mock delivery, Order Management Service, Inventory Service, KDS Service i Packing Service. Nie optymalizujemy jeszcze pod wieloregionową produkcję ani bardzo duże wolumeny danych.

Decyzja musi być zgodna z:

- [ADR 001: architektura mikroserwisów](./ADR-001-architecture.md)
- [ADR 002: .NET Aspire](./ADR-002-aspire.md)
- [ADR 003: asynchroniczna walidacja zamówień](./ADR-003-async-validation.md)
- [ADR 004: granice mikroserwisów](./ADR-004-service-boundries.md)
- [ADR 005: Event Aggregator w Packing Service](./ADR-005-event-aggregator.md)
- [ADR 007: Storefront Service](./ADR-007-storefront.md)
- [ADR 009: multi-tenancy marek](./ADR-009-mulit-tenancy.md)
- [ADR 010: Kitchen Stations](./ADR-010-kitchen-stations.md)

## 3. Decyzja

Wybieramy następujący stos technologiczny dla pierwszej wersji systemu:

| Obszar                    | Technologia                                                                  |
| :------------------------ | :--------------------------------------------------------------------------- |
| Backend                   | .NET 10 LTS, C#, ASP.NET Core Web API                                        |
| Orkiestracja lokalna      | .NET Aspire AppHost i ServiceDefaults                                        |
| Frontend                  | React 19, TypeScript, Vite                                                   |
| Aplikacje UI              | osobne aplikacje dla Storefront, Admin Panel, Kitchen App i Packing Terminal |
| Realtime                  | ASP.NET Core SignalR                                                         |
| Bazy danych               | PostgreSQL per service                                                       |
| Dostęp do danych          | EF Core z providerem Npgsql                                                  |
| Messaging                 | RabbitMQ                                                                     |
| Warstwa komunikacji       | WolverineFx z RabbitMQ, durable inbox/outbox i obsługą sag                   |
| Cache i stan krótkotrwały | Redis tylko tam, gdzie potrzeba sesji, cache lub backplane                   |
| Testy backendu            | xUnit, Aspire.Hosting.Testing                                                |
| Testy frontendu i E2E     | Playwright                                                                   |
| Obserwowalność            | OpenTelemetry przez Aspire ServiceDefaults i Aspire Dashboard                |

## 4. Uzasadnienie

### .NET 10 LTS i ASP.NET Core

.NET 10 LTS daje stabilną bazę na cały okres budowy MVP i pasuje do wcześniejszej decyzji o .NET Aspire. ASP.NET Core dobrze wspiera API HTTP, SignalR, health checki, OpenTelemetry, dependency injection oraz integrację z PostgreSQL i RabbitMQ.

### React, TypeScript i Vite

System wymaga kilku interfejsów o różnej charakterystyce: publicznego sklepu white-label, panelu managera, aplikacji tabletowej KDS i terminala wydawki. React z TypeScript daje wspólny język UI, dobry ekosystem i łatwe współdzielenie komponentów, a Vite upraszcza szybki lokalny feedback loop.

### RabbitMQ zamiast Kafka

RabbitMQ lepiej pasuje do MVP opartego na komendach, zdarzeniach domenowych, kolejkach roboczych, retry, dead letter queues i czytelnych przepływach między mikroserwisami. Kafka byłaby sensowna przy dużej retencji strumieni, replayu zdarzeń analitycznych i bardzo wysokim wolumenie, ale na tym etapie zwiększyłaby złożoność operacyjną bez proporcjonalnej korzyści.

### WolverineFx zamiast MassTransit v9

MassTransit jest dojrzałym rozwiązaniem, ale od wersji v9 przechodzi w model komercyjny. Dla edukacyjnego MVP ograniczamy koszt i ryzyko licencyjne. WolverineFx daje integrację z RabbitMQ, EF Core, PostgreSQL, durable inbox/outbox, sagami i testowaniem przepływów wiadomości, pozostając dobrym dopasowaniem do ekosystemu .NET.

### PostgreSQL per service

PostgreSQL jest domyślną bazą transakcyjną dla usług. Każdy mikroserwis ma własną bazę lub schemat zarządzany niezależnie, własne migracje i własny model danych. Nie ma wspólnej bazy aplikacyjnej między usługami.

### SignalR dla KDS i Packing

KDS i Packing wymagają natychmiastowej aktualizacji ekranów. SignalR upraszcza obsługę WebSocketów, reconnection, grup subskrypcyjnych dla stacji kuchennych i komunikację z aplikacjami React.

### Redis jako opcjonalny komponent

Redis nie jest domyślną bazą domenową. Używamy go tylko wtedy, gdy konkretny przypadek wymaga krótkotrwałego stanu, cache, sesji lub skalowania realtime przez backplane.

## 5. Konsekwencje

### Pozytywne

- Jeden główny ekosystem backendowy upraszcza onboarding i lokalne uruchamianie.
- Aspire zapewnia spójny AppHost, dashboard, service discovery, health checki i telemetrykę.
- RabbitMQ i WolverineFx wspierają wzorce potrzebne w projekcie: inbox/outbox, retry, dead letter queues i sagę zamówienia.
- React i TypeScript pozwalają tworzyć wszystkie aplikacje UI w jednym spójnym stacku.
- PostgreSQL per service wzmacnia autonomię mikroserwisów i zgodność z istniejącymi ADR-ami.

### Negatywne

- Stos .NET i Aspire ogranicza sensowność polyglot microservices w pierwszej wersji.
- WolverineFx jest mniej powszechny niż MassTransit, więc zespół musi świadomie trzymać się jego konwencji.
- RabbitMQ nie zastępuje platformy analitycznego event streamingu. Jeśli pojawi się potrzeba długiej retencji i replayu zdarzeń, może powstać osobna decyzja o Kafce.
- Kilka aplikacji React wymaga dyscypliny w zakresie współdzielonych komponentów, stylów i kontraktów API.

## 6. Adaptery integracyjne

Płatności i platformy delivery traktujemy jako adaptery na granicy systemu.

W v1 implementujemy:

- mock payment adapter w Storefront Service,
- mock delivery webhook adapter w Order Management Service,
- kontrakty umożliwiające późniejsze podpięcie Stripe, PayU, BLIK, Glovo, Uber Eats i Pyszne.pl.

Realne integracje nie są wymagane do domknięcia MVP.

## 7. Źródła

- [.NET support policy](https://dotnet.microsoft.com/platform/support-policy)
- [.NET Aspire documentation](https://learn.microsoft.com/en-us/dotnet/aspire)
- [RabbitMQ documentation](https://www.rabbitmq.com/docs)
- [PostgreSQL documentation](https://www.postgresql.org/docs/current/)
- [React versions](https://react.dev/versions)
- [Vite releases](https://vite.dev/releases)
- [WolverineFx EF Core integration](https://wolverinefx.io/guide/durability/efcore/)
- [ASP.NET Core SignalR documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
