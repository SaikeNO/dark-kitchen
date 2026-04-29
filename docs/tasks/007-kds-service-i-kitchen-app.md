# TASK 007: KDS Service i Kitchen App

## Cel

Zaimplementować proces przygotowania jedzenia: rozbijanie zamówienia na zadania kuchenne, routing na stacje i realtime UI dla kucharzy.

## Zakres

- Konsumpcja `OrderAccepted`.
- Read-model stacji i routingu z Catalog Service.
- Model `KitchenTicket` i `KitchenTask`.
- SignalR Hub z grupami dla stacji kuchennych.
- Kitchen App w React dla tabletów.
- Akcje kucharza: start zadania i zakończenie zadania.
- Publikacja `ItemPreparationStarted` i `ItemPreparationCompleted`.

## Poza zakresem

- Zaawansowane algorytmy harmonogramowania pracy kuchni.
- Tryb offline z pełną synchronizacją po odzyskaniu sieci.
- Integracje z fizycznymi ekranami KDS innych producentów.

## Zależności

- [TASK 002](./002-kontrakty-i-komunikacja-zdarzeniowa.md)
- [TASK 003](./003-catalog-recipe-service-i-admin-panel.md)
- [TASK 005](./005-order-management-service.md)
- [Opis KDS Service](../services/kitchen-display-system-service.md)
- [ADR 010](../adr/ADR-010-kitchen-stations.md)

## Kroki realizacji

1. Utworzyć bazę PostgreSQL KDS Service.
2. Zaimplementować read-model stacji i routingu produktów.
3. Po `OrderAccepted` tworzyć ticket i zadania dla właściwych stacji.
4. Dodać SignalR Hub z dołączaniem klienta do grupy stacji.
5. Zbudować Kitchen App z wyborem stacji po starcie.
6. Dodać akcje `Start` i `Done` dla zadań.
7. Publikować zdarzenia postępu przez outbox.
8. Dodać reconnect i odtworzenie listy zadań po odświeżeniu tabletu.

## Kryteria akceptacji

- Tablet widzi tylko zadania swojej stacji.
- Dwa tablety tej samej stacji widzą zsynchronizowany stan.
- Zakończenie zadania publikuje `ItemPreparationCompleted`.
- KDS nie pyta Catalog synchronicznie w trakcie obsługi zamówienia.
- Po reconnect Kitchen App wraca do poprzednio wybranej stacji.

## Scenariusze testowe

- Zamówienie z burgerem i frytkami tworzy zadania dla dwóch stacji.
- Kliknięcie `Done` na jednym tablecie aktualizuje drugi tablet tej samej stacji.
- Tablet stacji Grill nie widzi zadań Frytownicy.
- Zmiana routingu w Catalog wpływa na nowe zamówienia, bez zmiany otwartych ticketów.
