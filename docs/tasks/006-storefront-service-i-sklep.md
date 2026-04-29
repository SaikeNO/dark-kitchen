# TASK 006: Storefront Service i sklep

## Cel

Zbudować własny kanał sprzedaży: white-label sklep React, Storefront Service jako BFF oraz mock payment umożliwiający składanie zamówień do OMS.

## Zakres

- Rozpoznawanie marki po domenie, nagłówku lub parametrze developerskim.
- API menu dla sklepu, filtrowane po `BrandId`.
- Koszyk i checkout po stronie Storefront.
- Mock payment adapter z prostymi statusami `Pending`, `Success`, `Failed`.
- Przekazanie opłaconego zamówienia do OMS.
- Podstawowe konto klienta lub checkout gościa, zależnie od najprostszego przepływu MVP.
- Sklep React z listą produktów, koszykiem i ekranem potwierdzenia.

## Poza zakresem

- Produkcyjne logowanie społecznościowe.
- Realne płatności Stripe, PayU lub BLIK.
- Program lojalnościowy, kupony i reklamacje.

## Zależności

- [TASK 003](./003-catalog-recipe-service-i-admin-panel.md)
- [TASK 005](./005-order-management-service.md)
- [Opis Storefront Service](../services/store-front-service.md)
- [ADR 007](../adr/ADR-007-storefront.md)
- [ADR 009](../adr/ADR-009-mulit-tenancy.md)

## Kroki realizacji

1. Utworzyć bazę PostgreSQL Storefront Service.
2. Zaimplementować model koszyka, transakcji płatności i konfiguracji marki jako read-model.
3. Dodać mechanizm rozpoznawania `BrandId`.
4. Dodać endpoint menu pobierający lub czytający zsynchronizowane dane Catalog.
5. Dodać mock payment adapter sterowany konfiguracją lub parametrem testowym.
6. Dodać checkout, który po sukcesie płatności tworzy zamówienie w OMS.
7. Zbudować sklep React z dynamicznym brandingiem i prostym koszykiem.

## Kryteria akceptacji

- Użytkownik widzi wyłącznie menu swojej marki.
- Checkout z mock payment success tworzy zamówienie w OMS.
- Checkout z mock payment failed nie tworzy zamówienia.
- `BrandId` trafia do zamówienia i dalszych zdarzeń.
- Frontend white-label zmienia podstawowe logo/nazwę/kolory na podstawie konfiguracji marki.

## Scenariusze testowe

- Marka A i marka B mają różne menu przy tym samym kodzie sklepu.
- Koszyk odrzuca produkt niedostępny dla aktualnego `BrandId`.
- Płatność zakończona błędem zatrzymuje proces przed OMS.
- Odświeżenie strony nie miesza koszyka między markami.
