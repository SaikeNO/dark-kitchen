# TASK 010: Testy E2E, obserwowalność i demo

## Cel

Domknąć MVP jako możliwy do pokazania, testowany i obserwowalny przepływ end-to-end.

## Zakres

- Testy integracyjne przez Aspire.Hosting.Testing.
- Test E2E Playwright dla happy path sklepu i kuchni.
- Testy kontraktowe zdarzeń.
- Testy scenariuszy błędów: brak składników, duplikaty, out of order, reconnect i izolacja `BrandId`.
- Konfiguracja logów, trace, metryk i health checków.
- Seed danych demo i instrukcja uruchomienia prezentacji.

## Poza zakresem

- Pełne testy wydajnościowe.
- Produkcyjny monitoring w chmurze.
- Testy na realnych urządzeniach tabletowych poza przeglądarką desktop/mobile emulation.

## Zależności

- [TASK 001](./001-fundamenty-rozwiazania-i-aspire.md)
- [TASK 002](./002-kontrakty-i-komunikacja-zdarzeniowa.md)
- Wszystkie zadania domenowe od [TASK 003](./003-catalog-recipe-service-i-admin-panel.md) do [TASK 009](./009-integracje-zewnetrzne-platnosci-delivery.md)

## Kroki realizacji

1. Dodać projekt testów integracyjnych dla AppHost.
2. Przygotować seed danych demo: marka, produkty, składniki, receptury, stacje i stany magazynowe.
3. Dodać test happy path przez API i komunikację zdarzeniową.
4. Dodać Playwright E2E przechodzący przez sklep, KDS i Packing Terminal.
5. Dodać test braku składników i odrzucenia zamówienia.
6. Dodać testy idempotencji dla powtórzonych eventów.
7. Dodać test out of order dla Packing Service.
8. Upewnić się, że trace pokazuje jeden `CorrelationId` przez wszystkie serwisy.
9. Spisać scenariusz demo krok po kroku.

## Kryteria akceptacji

- Jedna komenda uruchamia środowisko demo.
- Test E2E potwierdza pełny przepływ od złożenia zamówienia do gotowości odbioru.
- Testy kontraktowe obejmują wszystkie kluczowe zdarzenia.
- Aspire Dashboard pokazuje trace przechodzący przez OMS, Inventory, KDS i Packing.
- Scenariusze błędów są odtwarzalne lokalnie.

## Scenariusze testowe

- Happy path: Storefront -> OMS -> Inventory -> KDS -> Packing.
- Brak składników: zamówienie kończy się `Rejected`.
- Duplikat `OrderPlaced` nie rezerwuje składników dwa razy.
- `ItemPreparationCompleted` przed manifestem zostaje poprawnie rozliczony.
- Rozłączenie Kitchen App i reconnect przywraca listę zadań.
- Marka A nie widzi produktów ani zamówień marki B.
