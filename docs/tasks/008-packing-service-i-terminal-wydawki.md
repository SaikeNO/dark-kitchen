# TASK 008: Packing Service i terminal wydawki

## Cel

Zaimplementować wydawkę jako agregator zdarzeń, który wie, kiedy wszystkie elementy zamówienia są gotowe do spakowania i odbioru.

## Zakres

- Konsumpcja `OrderAccepted` i utworzenie manifestu pakowania.
- Konsumpcja `ItemPreparationCompleted`.
- Model `PackingManifest`, `ManifestItem` i instrukcji pakowania.
- Obsługa zdarzeń out of order.
- Terminal wydawki w React z realtime aktualizacją przez SignalR.
- Publikacja `OrderReadyForPacking` i `OrderReadyForPickup`.
- Timeout lub alert dla zbyt długo niekompletnego manifestu.

## Poza zakresem

- Integracja z drukarkami termicznymi.
- Zaawansowana optymalizacja kolejki kurierów.
- Automatyczne wzywanie kuriera w realnej platformie delivery.

## Zależności

- [TASK 002](./002-kontrakty-i-komunikacja-zdarzeniowa.md)
- [TASK 005](./005-order-management-service.md)
- [TASK 007](./007-kds-service-i-kitchen-app.md)
- [Opis Packing Service](../services/packing-service.md)
- [ADR 005](../adr/ADR-005-event-aggregator.md)

## Kroki realizacji

1. Utworzyć bazę PostgreSQL Packing Service.
2. Po `OrderAccepted` tworzyć manifest z oczekiwanymi elementami.
3. Po `ItemPreparationCompleted` oznaczać odpowiedni element jako gotowy.
4. Obsłużyć race condition, gdy gotowy element przyjdzie przed manifestem.
5. Po skompletowaniu manifestu opublikować `OrderReadyForPacking`.
6. Dodać Terminal wydawki z listą zamówień i stanem kompletacji.
7. Po kliknięciu `Wydane` opublikować `OrderReadyForPickup`.
8. Dodać alert dla manifestów przekraczających ustalony czas oczekiwania.

## Kryteria akceptacji

- Packing poprawnie liczy kompletację manifestu.
- Zdarzenia z KDS są idempotentne.
- Terminal pokazuje statusy w czasie rzeczywistym.
- Out of order event nie gubi informacji o gotowym elemencie.
- OMS otrzymuje zdarzenie gotowości do odbioru.

## Scenariusze testowe

- Trzy elementy zamówienia kompletują manifest dopiero po trzecim `ItemPreparationCompleted`.
- Duplikat `ItemPreparationCompleted` nie zawyża licznika gotowych elementów.
- `ItemPreparationCompleted` przed `OrderAccepted` zostaje obsłużony po utworzeniu manifestu.
- Manifest niekompletny zbyt długo generuje alert na terminalu.
