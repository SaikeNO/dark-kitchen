# TASK 002: Kontrakty i komunikacja zdarzeniowa

## Cel

Ustanowić wspólny standard zdarzeń, routing przez RabbitMQ oraz trwałą obsługę publikacji i konsumpcji wiadomości przez WolverineFx.

## Zakres

- Utworzenie wspólnego projektu kontraktów zdarzeń.
- Zdefiniowanie minimalnej koperty eventu.
- Konfiguracja WolverineFx z RabbitMQ w serwisach.
- Konfiguracja durable inbox/outbox na PostgreSQL.
- Ustalenie nazw kolejek, exchange i konwencji wersjonowania kontraktów.
- Dodanie testów kontraktowych dla kluczowych zdarzeń.

## Poza zakresem

- Pełna implementacja domenowa konsumentów.
- Centralny schema registry.
- Kafka lub analityczny event streaming.

## Zależności

- [TASK 001](./001-fundamenty-rozwiazania-i-aspire.md)
- [ADR 003](../adr/ADR-003-async-validation.md)
- [ADR 011](../adr/ADR-011-stos-technologiczny.md)

## Kroki realizacji

1. Dodać projekt `Contracts` lub równoważny moduł współdzielony dla komunikatów.
2. Zdefiniować standard koperty: `EventId`, `EventType`, `OccurredAt`, `CorrelationId`, `CausationId`, `SchemaVersion`, `BrandId`.
3. Przyjąć JSON camelCase jako format serializacji.
4. Zdefiniować zdarzenia: `OrderPlaced`, `InventoryReserved`, `InventoryReservationFailed`, `OrderAccepted`, `ItemPreparationStarted`, `ItemPreparationCompleted`, `OrderReadyForPacking`, `OrderReadyForPickup`.
5. Skonfigurować WolverineFx w każdym serwisie, który publikuje lub konsumuje zdarzenia.
6. Skonfigurować trwały inbox/outbox per service na PostgreSQL.
7. Dodać idempotencję po `EventId` i korelację po `CorrelationId`.

## Kryteria akceptacji

- Każde zdarzenie ma stabilną nazwę, wersję i jawne pola wymagane.
- Publikacja zdarzenia jest atomowa względem zapisu lokalnej bazy przez outbox.
- Konsumpcja zdarzeń jest idempotentna i odporna na duplikaty.
- Każdy log dotyczący obsługi eventu zawiera `CorrelationId`.
- Testy kontraktowe wykrywają zmianę publicznego kształtu zdarzenia.

## Scenariusze testowe

- Serializacja i deserializacja każdego zdarzenia w JSON camelCase.
- Ponowne dostarczenie tego samego `EventId` nie wykonuje drugi raz skutku biznesowego.
- Brak RabbitMQ podczas zapisu nie gubi zdarzenia, tylko zostawia je w outbox.
- Zdarzenie bez wymaganego `CorrelationId` jest odrzucane lub trafia do obsługi błędów.
