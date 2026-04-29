# TASK 004: Inventory Service

## Cel

Zaimplementować magazyn składników, read-model receptur oraz rezerwację stanów pod zamówienia.

## Zakres

- Model danych składników, stanów, rezerwacji i logu zmian magazynowych.
- Konsumpcja zdarzeń receptur z Catalog Service.
- Obsługa `OrderPlaced` i publikacja `InventoryReserved` albo `InventoryReservationFailed`.
- Idempotentna rezerwacja składników po `OrderId`.
- Podstawowe endpointy administracyjne do dostaw i korekt stanów.

## Poza zakresem

- Prognozowanie braków.
- Integracje z dostawcami.
- Pełna księgowość magazynowa i wycena FIFO/LIFO.

## Zależności

- [TASK 002](./002-kontrakty-i-komunikacja-zdarzeniowa.md)
- [TASK 003](./003-catalog-recipe-service-i-admin-panel.md)
- [Opis Inventory Service](../services/inventory-service.md)
- [ADR 006](../adr/ADR-006-separate-master-data-and-stock.md)

## Kroki realizacji

1. Utworzyć bazę PostgreSQL i migracje Inventory Service.
2. Zaimplementować encje `WarehouseItem`, `RecipeSnapshot`, `StockReservation` i `InventoryLog`.
3. Dodać konsumenta zdarzeń receptur i aktualizację lokalnego read-modelu.
4. Dodać konsumenta `OrderPlaced`.
5. W jednej transakcji sprawdzić dostępność, zapisać rezerwacje i opublikować wynik przez outbox.
6. Dodać kompensację zwolnienia rezerwacji dla anulowanego zamówienia, jeśli będzie emitowane odpowiednie zdarzenie.
7. Dodać alert lub flagę dla składników poniżej progu minimalnego.

## Kryteria akceptacji

- Rezerwacja odejmuje dostępny stan bez trwałego zdejmowania fizycznego zużycia.
- Brak któregokolwiek składnika powoduje `InventoryReservationFailed` z powodem.
- Duplikat `OrderPlaced` nie tworzy podwójnej rezerwacji.
- Inventory działa na lokalnym `RecipeSnapshot`, bez synchronicznego odpytywania Catalog Service.
- Wszystkie zmiany stanu są zapisane w `InventoryLog`.

## Scenariusze testowe

- Zamówienie możliwe do realizacji kończy się `InventoryReserved`.
- Zamówienie z brakującym składnikiem kończy się `InventoryReservationFailed`.
- Dwa równoległe zamówienia nie rezerwują tego samego stanu ponad dostępny limit.
- Aktualizacja receptury zmienia zachowanie kolejnych rezerwacji, bez zmiany historycznych rezerwacji.
