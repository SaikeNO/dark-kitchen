# Roadmap zadań dla edukacyjnego MVP Dark Kitchen

Ten dokument porządkuje wysokopoziomową kolejność prac dla systemu Dark Kitchen. Szczegółowe opisy zadań znajdują się w plikach w folderze [tasks](./tasks).

## Założenia

- Priorytetem jest edukacyjne MVP uruchamiane lokalnie przez .NET Aspire.
- Stos technologiczny jest opisany w [ADR 011](./adr/ADR-011-stos-technologiczny.md).
- Nie zmieniamy istniejących ADR-ów ani ich numeracji.
- Pierwszy pełny przepływ biznesowy: Storefront lub mock delivery -> OMS -> Inventory -> KDS -> Packing.
- Realne płatności i realne platformy delivery zostają za adapterami i mogą być podmienione później.

## Kolejność realizacji

| Kolejność | Zadanie | Wynik | Zależności |
| :--- | :--- | :--- | :--- |
| 1 | [Fundamenty rozwiązania i Aspire](./tasks/001-fundamenty-rozwiazania-i-aspire.md) | Szkielet solution, AppHost, ServiceDefaults i projekty serwisów | ADR 001, ADR 002, ADR 011 |
| 2 | [Kontrakty i komunikacja zdarzeniowa](./tasks/002-kontrakty-i-komunikacja-zdarzeniowa.md) | Wspólny standard eventów, RabbitMQ, WolverineFx, inbox/outbox | Zadanie 1 |
| 3 | [Catalog & Recipe Service i Admin Panel](./tasks/003-catalog-recipe-service-i-admin-panel.md) | Marki, menu, receptury i stacje kuchenne jako master data | Zadania 1-2 |
| 4 | [Inventory Service](./tasks/004-inventory-service.md) | Stany, receptury read-model i rezerwacje składników | Zadania 2-3 |
| 5 | [Order Management Service](./tasks/005-order-management-service.md) | Przyjęcie zamówień, stan zamówienia i saga | Zadania 2-4 |
| 6 | [Storefront Service i sklep](./tasks/006-storefront-service-i-sklep.md) | White-label storefront, koszyk, mock payment i zamówienie | Zadania 3, 5 |
| 7 | [KDS Service i Kitchen App](./tasks/007-kds-service-i-kitchen-app.md) | Zadania kuchenne, SignalR, grupy stacji i bump flow | Zadania 2-5 |
| 8 | [Packing Service i terminal wydawki](./tasks/008-packing-service-i-terminal-wydawki.md) | Manifest pakowania i agregacja gotowych elementów | Zadania 2, 5, 7 |
| 9 | [Integracje zewnętrzne: płatności i delivery](./tasks/009-integracje-zewnetrzne-platnosci-delivery.md) | Adaptery mock i kontrakty pod realne integracje | Zadania 5-6 |
| 10 | [Testy E2E, obserwowalność i demo](./tasks/010-testy-e2e-obserwowalnosc-i-demo.md) | Testowany i obserwowalny przepływ demo end-to-end | Zadania 1-9 |

## Minimalny happy path MVP

1. Manager tworzy markę, produkt, recepturę i routing do stacji w Catalog Service.
2. Storefront pobiera menu dla `BrandId` i składa opłacone zamówienie przez mock payment.
3. OMS zapisuje zamówienie i publikuje `OrderPlaced`.
4. Inventory rezerwuje składniki i publikuje `InventoryReserved`.
5. OMS zmienia status i publikuje `OrderAccepted`.
6. KDS tworzy zadania kuchenne i wysyła je do właściwych grup SignalR.
7. Kucharz oznacza elementy jako rozpoczęte i gotowe.
8. Packing agreguje `ItemPreparationCompleted`, oznacza zamówienie jako gotowe do pakowania i publikuje `OrderReadyForPickup`.
9. OMS zamyka zamówienie jako gotowe do odbioru lub zakończone.

## Scenariusze ryzyka do pokrycia

- Brak składników powoduje `InventoryReservationFailed` i odrzucenie zamówienia.
- Duplikat zdarzenia nie podwaja rezerwacji ani zadań kuchennych.
- Zdarzenie przychodzi out of order, np. `ItemPreparationCompleted` przed manifestem pakowania.
- Tablet KDS traci połączenie i po reconnect wraca do właściwej grupy stacji.
- Dane jednej marki nie wyciekają do innej marki przy filtracji po `BrandId`.

## Definicja ukończenia MVP

- Wszystkie serwisy i aplikacje UI uruchamiają się z AppHost.
- Pełny happy path jest pokryty testem E2E.
- Krytyczne zdarzenia mają testy kontraktowe.
- Aspire Dashboard pokazuje logi, trace i health checki głównych komponentów.
- Dokumentacja zadań opisuje obecny zakres oraz świadomie pozostawione ograniczenia.
