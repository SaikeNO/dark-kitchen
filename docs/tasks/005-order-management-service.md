# TASK 005: Order Management Service

## Cel

Zbudować centralny hub cyklu życia zamówienia, który przyjmuje zamówienia, normalizuje je, prowadzi stan i koordynuje sagę biznesową.

## Zakres

- API przyjmowania zamówień ze Storefront Service.
- Mock webhook delivery jako wejście dla platform zewnętrznych.
- Model `Order`, `OrderItem`, `OrderHistory` i snapshot klienta.
- Stan zamówienia: `Placed`, `Accepted`, `Preparing`, `ReadyForPacking`, `ReadyForPickup`, `Completed`, `Rejected`, `Cancelled`.
- Publikacja `OrderPlaced` i `OrderAccepted`.
- Konsumpcja wyników Inventory, KDS i Packing.
- Logika kompensacyjna dla braku składników.

## Poza zakresem

- Realne webhooki Glovo, Uber Eats i Pyszne.pl.
- Realne zwroty płatności.
- Zaawansowany dashboard analityczny.

## Zależności

- [TASK 002](./002-kontrakty-i-komunikacja-zdarzeniowa.md)
- [TASK 004](./004-inventory-service.md)
- [Opis Order Management Service](../services/order-managment-service.md)
- [ADR 003](../adr/ADR-003-async-validation.md)

## Kroki realizacji

1. Utworzyć bazę PostgreSQL i migracje OMS.
2. Zaimplementować model zamówienia i historię zmian statusów.
3. Dodać adapter wejściowy dla Storefront.
4. Dodać mock delivery webhook i mapowanie do wspólnego modelu zamówienia.
5. Publikować `OrderPlaced` po zapisaniu zamówienia.
6. Obsłużyć `InventoryReserved` zmianą statusu i publikacją `OrderAccepted`.
7. Obsłużyć `InventoryReservationFailed` zmianą statusu na `Rejected`.
8. Obsłużyć zdarzenia z KDS i Packing, aktualizując globalny status zamówienia.

## Kryteria akceptacji

- OMS zapisuje każde przyjęte zamówienie przed publikacją zdarzenia.
- Każda zmiana statusu dopisuje wpis do historii.
- `BrandId`, `OrderId` i `CorrelationId` są przekazywane przez cały przepływ.
- Zamówienie z brakiem składników jest odrzucane i nie trafia do KDS.
- API wejściowe jest odporne na powtórzone żądanie z tym samym zewnętrznym identyfikatorem.

## Scenariusze testowe

- Zamówienie ze Storefront przechodzi z `Placed` do `Accepted` po `InventoryReserved`.
- Mock delivery webhook tworzy zamówienie w tym samym modelu co Storefront.
- `InventoryReservationFailed` kończy zamówienie statusem `Rejected`.
- Powtórzony webhook delivery nie tworzy drugiego zamówienia.
