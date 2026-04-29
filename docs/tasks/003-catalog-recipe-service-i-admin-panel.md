# TASK 003: Catalog & Recipe Service i Admin Panel

## Cel

Zbudować źródło prawdy dla marek, menu, produktów, receptur i stacji kuchennych oraz panel administracyjny do zarządzania tymi danymi.

## Zakres

- Model danych dla `Brand`, `Product`, `Category`, `Ingredient`, `Recipe` i `Station`.
- API administracyjne do zarządzania master data.
- API odczytowe dla Storefront i read-modeli innych serwisów.
- Publikacja zdarzeń o zmianach menu, cen, receptur i stacji.
- Admin Panel w React do podstawowego CRUD.
- Walidacje zapobiegające publikacji produktu bez ceny, marki lub receptury.

## Poza zakresem

- Zaawansowany drag and drop edytor menu.
- Upload produkcyjnych zdjęć do zewnętrznego storage.
- Rozbudowane role administracyjne poza prostym podziałem manager/operator.

## Zależności

- [TASK 001](./001-fundamenty-rozwiazania-i-aspire.md)
- [TASK 002](./002-kontrakty-i-komunikacja-zdarzeniowa.md)
- [Opis Catalog & Recipe Service](../services/catalog-and-recipe-service.md)
- [ADR 008](../adr/ADR-008-catalog-service.md)

## Kroki realizacji

1. Utworzyć bazę PostgreSQL i migracje Catalog Service.
2. Zaimplementować encje marek, produktów, składników, receptur i stacji.
3. Dodać endpointy administracyjne z walidacją wejścia.
4. Dodać endpoint odczytu menu po `BrandId`.
5. Publikować zdarzenia zmian master data przez outbox.
6. Zbudować Admin Panel z widokami: marki, menu, receptury i stacje.
7. Dodać seed danych demo dla co najmniej jednej marki i jednej stacji kuchennej.

## Kryteria akceptacji

- Manager może utworzyć aktywną markę, produkt, recepturę i przypisać produkt do stacji.
- Storefront może pobrać menu wyłącznie dla wskazanego `BrandId`.
- Inventory otrzymuje zdarzenie o recepturze potrzebne do read-modelu.
- KDS otrzymuje dane stacji i routingu kuchennego.
- Nie można aktywować produktu bez wymaganych danych biznesowych.

## Scenariusze testowe

- Utworzenie produktu i receptury publikuje zdarzenie dla Inventory.
- Zmiana ceny publikuje zdarzenie dla read-modeli sprzedażowych.
- Produkt marki A nie pojawia się w menu marki B.
- Usunięcie lub dezaktywacja stacji nie psuje historycznych zamówień.
