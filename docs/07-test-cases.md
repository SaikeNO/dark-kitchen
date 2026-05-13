# Test case'y dla use case'ow

Dokument mapuje test case'y na use case'y z `docs/06-use-cases.md`. To backlog i mapa pokrycia testowego. Kolumna `Automatyzacja` mowi, czy scenariusz ma widoczny odpowiednik w repo, czy jest propozycja do dopisania.

## Legenda

| Pole | Znaczenie |
| --- | --- |
| TC ID | Stabilny identyfikator test case'a. |
| UC | Use case z `docs/06-use-cases.md`. |
| Warunek | Dane, rola, stan systemu albo zdarzenie wejsciowe. |
| Akcja | Wywolanie endpointu, handlera, UI albo przeplywu. |
| Oczekiwany wynik | Minimalna asercja biznesowa. |
| Automatyzacja | `istnieje`, `czesciowo`, `do dodania`. |

## Catalog & Recipe Service

| TC ID | UC | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-CAT-001-01 | UC-CAT-001 | Poprawne konto operatora albo managera. | `POST /api/admin/auth/login`. | Zwraca sesje, role i ustawia cookie. | istnieje |
| TC-CAT-001-02 | UC-CAT-001 | Zle haslo albo nieznany email. | `POST /api/admin/auth/login`. | Zwraca blad logowania bez sesji. | istnieje |
| TC-CAT-002-01 | UC-CAT-002 | Zalogowany user. | `POST /api/admin/auth/logout`. | Cookie sesji wyczyszczone, kolejne `/me` zwraca 401. | istnieje |
| TC-CAT-002-02 | UC-CAT-002 | Brak sesji. | `POST /api/admin/auth/logout`. | Zwraca 401 albo brak zmiany stanu. | istnieje |
| TC-CAT-003-01 | UC-CAT-003 | Zalogowany operator. | `GET /api/admin/auth/me`. | Zwraca usera i role. | istnieje |
| TC-CAT-003-02 | UC-CAT-003 | Brak sesji. | `GET /api/admin/auth/me`. | Zwraca 401. | istnieje |
| TC-CAT-004-01 | UC-CAT-004 | Operator zalogowany. | `GET /api/admin/brands`. | Zwraca liste marek. | istnieje |
| TC-CAT-004-02 | UC-CAT-004 | Brak sesji. | `GET /api/admin/brands`. | Zwraca 401. | istnieje |
| TC-CAT-005-01 | UC-CAT-005 | Manager zalogowany, poprawny payload. | `POST /api/admin/brands`. | Tworzy marke i publikuje `brand.changed`. | istnieje |
| TC-CAT-005-02 | UC-CAT-005 | Operator bez roli manager. | `POST /api/admin/brands`. | Zwraca 403. | istnieje |
| TC-CAT-005-03 | UC-CAT-005 | Manager, pusta nazwa. | `POST /api/admin/brands`. | Zwraca validation problem. | istnieje |
| TC-CAT-006-01 | UC-CAT-006 | Manager, istniejaca marka. | `PUT /api/admin/brands/{brandId}`. | Aktualizuje dane i publikuje `brand.changed`. | istnieje |
| TC-CAT-006-02 | UC-CAT-006 | Manager, nieznana marka. | `PUT /api/admin/brands/{brandId}`. | Zwraca 404. | istnieje |
| TC-CAT-006-03 | UC-CAT-006 | Operator bez roli manager. | `PUT /api/admin/brands/{brandId}`. | Zwraca 403. | do dodania |
| TC-CAT-007-01 | UC-CAT-007 | Manager, aktywna marka. | `POST /api/admin/brands/{brandId}/deactivate`. | Marka nieaktywna i `brand.changed` opublikowane. | istnieje |
| TC-CAT-007-02 | UC-CAT-007 | Manager, nieznana marka. | `POST /api/admin/brands/{brandId}/deactivate`. | Zwraca 404. | istnieje |
| TC-CAT-008-01 | UC-CAT-008 | Operator zalogowany. | `GET /api/admin/categories`. | Zwraca liste kategorii. | istnieje |
| TC-CAT-008-02 | UC-CAT-008 | Brak sesji. | `GET /api/admin/categories`. | Zwraca 401. | istnieje |
| TC-CAT-009-01 | UC-CAT-009 | Manager, poprawny brand i payload. | `POST /api/admin/categories`. | Tworzy kategorie i publikuje `category.changed`. | istnieje |
| TC-CAT-009-02 | UC-CAT-009 | Manager, pusta nazwa albo niepoprawny brand. | `POST /api/admin/categories`. | Zwraca validation problem. | istnieje |
| TC-CAT-009-03 | UC-CAT-009 | Operator bez roli manager. | `POST /api/admin/categories`. | Zwraca 403. | istnieje |
| TC-CAT-010-01 | UC-CAT-010 | Manager, istniejaca kategoria. | `PUT /api/admin/categories/{categoryId}`. | Aktualizuje kategorie i publikuje `category.changed`. | istnieje |
| TC-CAT-010-02 | UC-CAT-010 | Manager, nieznana kategoria. | `PUT /api/admin/categories/{categoryId}`. | Zwraca 404. | istnieje |
| TC-CAT-011-01 | UC-CAT-011 | Manager, aktywna kategoria. | `POST /api/admin/categories/{categoryId}/deactivate`. | Kategoria nieaktywna i `category.changed` opublikowane. | istnieje |
| TC-CAT-011-02 | UC-CAT-011 | Manager, nieznana kategoria. | `POST /api/admin/categories/{categoryId}/deactivate`. | Zwraca 404. | istnieje |
| TC-CAT-012-01 | UC-CAT-012 | Operator zalogowany. | `GET /api/admin/products`. | Zwraca produkty, mozliwie filtrowane po marce. | istnieje |
| TC-CAT-012-02 | UC-CAT-012 | Brak sesji. | `GET /api/admin/products`. | Zwraca 401. | istnieje |
| TC-CAT-013-01 | UC-CAT-013 | Manager, produkt w kategorii tej samej marki. | `POST /api/admin/products`. | Tworzy produkt i publikuje `menu.item_changed`. | istnieje |
| TC-CAT-013-02 | UC-CAT-013 | Kategoria z innej marki. | `POST /api/admin/products`. | Zwraca validation problem. | istnieje |
| TC-CAT-013-03 | UC-CAT-013 | Operator bez roli manager. | `POST /api/admin/products`. | Zwraca 403. | istnieje |
| TC-CAT-014-01 | UC-CAT-014 | Manager, istniejacy produkt. | `PUT /api/admin/products/{productId}`. | Aktualizuje produkt i publikuje `menu.item_changed`; przy zmianie ceny publikuje `product.price_changed`. | istnieje |
| TC-CAT-014-02 | UC-CAT-014 | Manager, nieznany produkt. | `PUT /api/admin/products/{productId}`. | Zwraca 404. | istnieje |
| TC-CAT-015-01 | UC-CAT-015 | Manager, produkt ma recepture i routing stacji. | `POST /api/admin/products/{productId}/activate`. | Produkt aktywny, zdarzenia katalogowe opublikowane. | istnieje |
| TC-CAT-015-02 | UC-CAT-015 | Produkt bez receptury albo routingu. | `POST /api/admin/products/{productId}/activate`. | Zwraca validation problem. | istnieje |
| TC-CAT-016-01 | UC-CAT-016 | Manager, aktywny produkt. | `POST /api/admin/products/{productId}/deactivate`. | Produkt nieaktywny i `menu.item_changed` opublikowane. | istnieje |
| TC-CAT-016-02 | UC-CAT-016 | Manager, nieznany produkt. | `POST /api/admin/products/{productId}/deactivate`. | Zwraca 404. | istnieje |
| TC-CAT-017-01 | UC-CAT-017 | Operator zalogowany. | `GET /api/admin/ingredients`. | Zwraca skladniki. | istnieje |
| TC-CAT-017-02 | UC-CAT-017 | Brak sesji. | `GET /api/admin/ingredients`. | Zwraca 401. | istnieje |
| TC-CAT-018-01 | UC-CAT-018 | Manager, poprawny payload. | `POST /api/admin/ingredients`. | Tworzy skladnik. | istnieje |
| TC-CAT-018-02 | UC-CAT-018 | Pusta jednostka albo nazwa. | `POST /api/admin/ingredients`. | Zwraca validation problem. | istnieje |
| TC-CAT-018-03 | UC-CAT-018 | Operator bez roli manager. | `POST /api/admin/ingredients`. | Zwraca 403. | istnieje |
| TC-CAT-019-01 | UC-CAT-019 | Manager, istniejacy skladnik. | `PUT /api/admin/ingredients/{ingredientId}`. | Aktualizuje skladnik. | istnieje |
| TC-CAT-019-02 | UC-CAT-019 | Manager, nieznany skladnik. | `PUT /api/admin/ingredients/{ingredientId}`. | Zwraca 404. | istnieje |
| TC-CAT-020-01 | UC-CAT-020 | Manager, aktywny skladnik. | `POST /api/admin/ingredients/{ingredientId}/deactivate`. | Skladnik nieaktywny. | istnieje |
| TC-CAT-020-02 | UC-CAT-020 | Manager, nieznany skladnik. | `POST /api/admin/ingredients/{ingredientId}/deactivate`. | Zwraca 404. | istnieje |
| TC-CAT-021-01 | UC-CAT-021 | Operator, produkt z receptura. | `GET /api/admin/products/{productId}/recipe`. | Zwraca recepture z pozycjami. | istnieje |
| TC-CAT-021-02 | UC-CAT-021 | Operator, nieznany produkt. | `GET /api/admin/products/{productId}/recipe`. | Zwraca 404. | istnieje |
| TC-CAT-022-01 | UC-CAT-022 | Manager, poprawne skladniki. | `PUT /api/admin/products/{productId}/recipe`. | Zapisuje recepture i publikuje `recipe.changed`. | istnieje |
| TC-CAT-022-02 | UC-CAT-022 | Duplikaty skladnikow. | `PUT /api/admin/products/{productId}/recipe`. | Zwraca validation problem. | istnieje |
| TC-CAT-022-03 | UC-CAT-022 | Operator bez roli manager. | `PUT /api/admin/products/{productId}/recipe`. | Zwraca 403. | istnieje |
| TC-CAT-023-01 | UC-CAT-023 | Operator zalogowany. | `GET /api/admin/stations`. | Zwraca stacje. | istnieje |
| TC-CAT-023-02 | UC-CAT-023 | Brak sesji. | `GET /api/admin/stations`. | Zwraca 401. | istnieje |
| TC-CAT-024-01 | UC-CAT-024 | Manager, poprawny payload. | `POST /api/admin/stations`. | Tworzy stacje i publikuje `station.changed`. | istnieje |
| TC-CAT-024-02 | UC-CAT-024 | Pusty kolor wyswietlania. | `POST /api/admin/stations`. | Zwraca validation problem. | istnieje |
| TC-CAT-024-03 | UC-CAT-024 | Operator bez roli manager. | `POST /api/admin/stations`. | Zwraca 403. | istnieje |
| TC-CAT-025-01 | UC-CAT-025 | Manager, istniejaca stacja. | `PUT /api/admin/stations/{stationId}`. | Aktualizuje stacje i publikuje `station.changed`. | istnieje |
| TC-CAT-025-02 | UC-CAT-025 | Manager, nieznana stacja. | `PUT /api/admin/stations/{stationId}`. | Zwraca 404. | istnieje |
| TC-CAT-026-01 | UC-CAT-026 | Manager, aktywna stacja. | `POST /api/admin/stations/{stationId}/deactivate`. | Stacja nieaktywna i `station.changed` opublikowane. | istnieje |
| TC-CAT-026-02 | UC-CAT-026 | Manager, nieznana stacja. | `POST /api/admin/stations/{stationId}/deactivate`. | Zwraca 404. | istnieje |
| TC-CAT-027-01 | UC-CAT-027 | Manager, produkt i aktywna stacja. | `PUT /api/admin/products/{productId}/station-route`. | Zapisuje routing i publikuje `product.station_routing_changed`. | istnieje |
| TC-CAT-027-02 | UC-CAT-027 | Stacja nieaktywna. | `PUT /api/admin/products/{productId}/station-route`. | Zwraca validation problem. | istnieje |
| TC-CAT-027-03 | UC-CAT-027 | Operator bez roli manager. | `PUT /api/admin/products/{productId}/station-route`. | Zwraca 403. | istnieje |
| TC-CAT-028-01 | UC-CAT-028 | Manager, poprawny plik i kind. | `POST /api/admin/uploads/{kind}`. | Zapisuje plik i zwraca URL. | istnieje |
| TC-CAT-028-02 | UC-CAT-028 | Brak pliku albo niedozwolony kind/format. | `POST /api/admin/uploads/{kind}`. | Zwraca validation problem. | do dodania |
| TC-CAT-029-01 | UC-CAT-029 | Istniejacy asset. | `GET /uploads/{kind}/{fileName}`. | Zwraca plik z poprawnym content type. | istnieje |
| TC-CAT-029-02 | UC-CAT-029 | Nieznany plik. | `GET /uploads/{kind}/{fileName}`. | Zwraca 404. | istnieje |
| TC-CAT-030-01 | UC-CAT-030 | Aktywna marka z menu. | `GET /api/menu/brands/{brandId}`. | Zwraca publiczne menu bez danych admin. | istnieje |
| TC-CAT-030-02 | UC-CAT-030 | Nieznana marka. | `GET /api/menu/brands/{brandId}`. | Zwraca 404. | istnieje |
| TC-CAT-031-01 | UC-CAT-031 | Manager w panelu. | Przejscie przez marki, menu, receptury, skladniki, stacje. | UI laduje dane i wysyla poprawne mutacje. | istnieje |
| TC-CAT-031-02 | UC-CAT-031 | Operator w panelu. | Proba wejscia w akcje zapisu. | UI blokuje albo backend zwraca 403. | istnieje |
| TC-CAT-032-01 | UC-CAT-032 | Serwis uruchomiony. | `GET /`, `GET /api/info`. | Zwraca status i odpowiedzialnosci. | istnieje |

## Storefront Service

| TC ID | UC | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-SF-001-01 | UC-SF-001 | Read model ma aktywne marki. | `GET /api/storefront/brands`. | Zwraca tylko marki dostepne dla sklepu. | istnieje |
| TC-SF-001-02 | UC-SF-001 | Brak aktywnych marek. | `GET /api/storefront/brands`. | Zwraca pusta liste. | do dodania |
| TC-SF-002-01 | UC-SF-002 | Poprawny `brandId`. | `GET /api/storefront/context`. | Zwraca kontekst marki i motyw. | istnieje |
| TC-SF-002-02 | UC-SF-002 | Nieznany `brandId`. | `GET /api/storefront/context`. | Zwraca 404 albo validation problem. | do dodania |
| TC-SF-003-01 | UC-SF-003 | Marka ma kategorie i produkty aktywne. | `GET /api/storefront/menu`. | Zwraca menu pogrupowane kategoriami. | istnieje |
| TC-SF-003-02 | UC-SF-003 | Produkt/kategoria nieaktywna. | `GET /api/storefront/menu`. | Nie pokazuje nieaktywnych elementow. | czesciowo |
| TC-SF-004-01 | UC-SF-004 | Nowy email klienta. | `POST /api/storefront/auth/register`. | Tworzy konto i loguje klienta. | istnieje |
| TC-SF-004-02 | UC-SF-004 | Istniejacy email albo slabe haslo. | `POST /api/storefront/auth/register`. | Zwraca validation problem. | do dodania |
| TC-SF-005-01 | UC-SF-005 | Istniejacy klient i poprawne haslo. | `POST /api/storefront/auth/login`. | Tworzy sesje klienta. | istnieje |
| TC-SF-005-02 | UC-SF-005 | Zle haslo. | `POST /api/storefront/auth/login`. | Zwraca blad logowania. | do dodania |
| TC-SF-006-01 | UC-SF-006 | Klient zalogowany. | `GET /api/storefront/auth/me`. | Zwraca aktualnego klienta. | istnieje |
| TC-SF-006-02 | UC-SF-006 | Brak sesji. | `GET /api/storefront/auth/me`. | Zwraca `null` albo 401 zgodnie z kontraktem. | do dodania |
| TC-SF-007-01 | UC-SF-007 | Klient zalogowany. | `POST /api/storefront/auth/logout`. | Sesja wyczyszczona. | istnieje |
| TC-SF-008-01 | UC-SF-008 | Poprawny `brandId`, brak `cartId`. | `POST /api/storefront/carts`. | Tworzy pusty koszyk. | istnieje |
| TC-SF-008-02 | UC-SF-008 | Istniejacy `cartId`. | `POST /api/storefront/carts`. | Odtwarza albo zwraca koszyk bez duplikacji. | czesciowo |
| TC-SF-009-01 | UC-SF-009 | Istniejacy koszyk. | `GET /api/storefront/carts/{cartId}`. | Zwraca pozycje i sumy. | istnieje |
| TC-SF-009-02 | UC-SF-009 | Nieznany koszyk. | `GET /api/storefront/carts/{cartId}`. | Zwraca 404. | istnieje |
| TC-SF-010-01 | UC-SF-010 | Aktywny produkt w menu. | `PATCH /api/storefront/carts/{cartId}`. | Zastepuje pozycje i przelicza sumy. | istnieje |
| TC-SF-010-02 | UC-SF-010 | Niedostepny produkt. | `PATCH /api/storefront/carts/{cartId}`. | Zwraca validation problem. | istnieje |
| TC-SF-010-03 | UC-SF-010 | Ilosc 0. | `PATCH /api/storefront/carts/{cartId}`. | Usuwa pozycje z koszyka. | do dodania |
| TC-SF-011-01 | UC-SF-011 | Koszyk z pozycjami, mock platnosc success. | `POST /api/storefront/checkout`. | Tworzy order w OMS i zwraca `orderId`. | istnieje |
| TC-SF-011-02 | UC-SF-011 | Mock platnosc failed. | `POST /api/storefront/checkout`. | Nie tworzy orderu i zwraca failure reason. | istnieje |
| TC-SF-011-03 | UC-SF-011 | Pusty koszyk. | `POST /api/storefront/checkout`. | Zwraca validation problem. | do dodania |
| TC-SF-012-01 | UC-SF-012 | `brand.changed` dla nowej marki. | Handler zdarzenia. | Read model marki utworzony/zaktualizowany. | do dodania |
| TC-SF-012-02 | UC-SF-012 | `brand.changed` z `IsActive=false`. | Handler zdarzenia. | Marka znika z publicznej listy. | do dodania |
| TC-SF-013-01 | UC-SF-013 | `category.changed`. | Handler zdarzenia. | Read model kategorii utworzony/zaktualizowany. | do dodania |
| TC-SF-014-01 | UC-SF-014 | `menu.item_changed`. | Handler zdarzenia. | Produkt w read modelu utworzony/zaktualizowany. | do dodania |
| TC-SF-014-02 | UC-SF-014 | `product.price_changed`. | Handler zdarzenia. | Cena produktu zmieniona bez utraty reszty danych. | do dodania |
| TC-SF-015-01 | UC-SF-015 | Sklep w przegladarce. | Wybor marki, dodanie produktu, checkout. | UI wykonuje pelny przeplyw. | do dodania |
| TC-SF-015-02 | UC-SF-015 | Sesja klienta. | Rejestracja, logowanie, logout w dialogu. | UI aktualizuje stan sesji. | do dodania |
| TC-SF-016-01 | UC-SF-016 | Serwis uruchomiony. | `GET /`, `GET /api/info`. | Zwraca status i odpowiedzialnosci. | istnieje |
| TC-SF-017-01 | UC-SF-017 | Checkout success utworzyl lokalny read model. | `GET /api/storefront/orders/{orderId}`. | Zwraca order klienta ze statusem `Placed`. | istnieje |
| TC-SF-017-02 | UC-SF-017 | Przychodza eventy lifecycle. | Handlery `order.accepted`, `order.ready_for_pickup`, `order.completed`. | Read model statusu przechodzi do najnowszego stanu i zachowuje pickup code. | istnieje |
| TC-SF-017-03 | UC-SF-017 | Storefront UI po checkout. | Polling statusu zamowienia. | UI pokazuje aktualny status zamowienia klienta. | istnieje |

## Order Management Service

| TC ID | UC | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-OMS-001-01 | UC-OMS-001 | Poprawne zamowienie storefront. | `POST /api/orders/storefront`. | Zapisuje order `Placed`, zwraca summary, publikuje `order.placed`. | istnieje |
| TC-OMS-001-02 | UC-OMS-001 | Puste pozycje albo niepoprawne dane klienta. | `POST /api/orders/storefront`. | Zwraca validation problem. | istnieje |
| TC-OMS-001-03 | UC-OMS-001 | Duplikat external/order id. | `POST /api/orders/storefront`. | Nie dubluje zamowienia. | do dodania |
| TC-OMS-002-01 | UC-OMS-002 | Mock delivery z obslugiwanej platformy. | `POST /api/mock-delivery/webhooks/orders`. | Tworzy order i mapuje source channel. | istnieje |
| TC-OMS-002-02 | UC-OMS-002 | Duplikat external order id. | `POST /api/mock-delivery/webhooks/orders`. | Zwraca istniejacy order. | istnieje |
| TC-OMS-002-03 | UC-OMS-002 | Brak platformy, nieznana platforma albo status. | `POST /api/mock-delivery/webhooks/orders`. | Zwraca validation problem. | istnieje |
| TC-OMS-002-04 | UC-OMS-002 | Puste pozycje. | `POST /api/mock-delivery/webhooks/orders`. | Zwraca validation problem. | istnieje |
| TC-OMS-003-01 | UC-OMS-003 | Istniejacy order. | `GET /api/orders/{orderId}`. | Zwraca szczegoly, pozycje i historie. | istnieje |
| TC-OMS-003-02 | UC-OMS-003 | Nieznany order. | `GET /api/orders/{orderId}`. | Zwraca 404. | istnieje |
| TC-OMS-004-01 | UC-OMS-004 | Ten sam input z roznych kanalow. | Create order handler. | Normalizuje do wspolnego modelu `Order`. | czesciowo |
| TC-OMS-004-02 | UC-OMS-004 | Walidacja domenowa nie przechodzi. | Create order handler. | Nie zapisuje orderu. | czesciowo |
| TC-OMS-005-01 | UC-OMS-005 | Order zapisany jako `Placed`. | Publikacja zdarzenia. | `order.placed` ma order id, source channel i pozycje. | istnieje |
| TC-OMS-005-02 | UC-OMS-005 | Envelope tworzony. | Serializacja kontraktu. | Event type i schema version stabilne. | istnieje |
| TC-OMS-006-01 | UC-OMS-006 | Order `Placed`, przychodzi `inventory.reserved`. | Handler. | Status `Accepted`, historia dopisana, `order.accepted` opublikowane. | istnieje |
| TC-OMS-006-02 | UC-OMS-006 | Order nie istnieje albo nie jest `Placed`. | Handler. | Brak zmiany i brak publikacji. | istnieje |
| TC-OMS-007-01 | UC-OMS-007 | Order `Placed`, przychodzi `inventory.reservation_failed`. | Handler. | Status `Rejected`, historia z reason code. | istnieje |
| TC-OMS-007-02 | UC-OMS-007 | Order nie istnieje albo status pozniejszy. | Handler. | Brak zmiany. | istnieje |
| TC-OMS-008-01 | UC-OMS-008 | `menu.item_changed` dla nowego produktu. | Handler. | Snapshot produktu utworzony/zaktualizowany. | do dodania |
| TC-OMS-008-02 | UC-OMS-008 | `menu.item_changed` z `IsActive=false`. | Handler. | Snapshot oznaczony niedostepny. | do dodania |
| TC-OMS-009-01 | UC-OMS-009 | `product.price_changed`. | Handler. | Cena snapshotu zaktualizowana. | do dodania |
| TC-OMS-010-01 | UC-OMS-010 | Order `Accepted`, `item.preparation_started`. | Handler. | Status przechodzi na `Preparing`. | istnieje |
| TC-OMS-010-02 | UC-OMS-010 | Order juz `Preparing` albo dalej. | Handler. | Handler idempotentny, nie cofa statusu. | istnieje |
| TC-OMS-011-01 | UC-OMS-011 | Order w toku, `order.ready_for_packing`. | Handler. | Status `ReadyForPacking`. | istnieje |
| TC-OMS-011-02 | UC-OMS-011 | Order `Rejected` albo `Cancelled`. | Handler. | Brak zmiany. | istnieje |
| TC-OMS-012-01 | UC-OMS-012 | Order w toku, `order.ready_for_pickup`. | Handler. | Status `ReadyForPickup`. | istnieje |
| TC-OMS-012-02 | UC-OMS-012 | Event powtorzony. | Handler. | Brak duplikacji historii albo brak cofania statusu. | do dodania |
| TC-OMS-013-01 | UC-OMS-013 | Serwis uruchomiony. | `GET /`, `GET /api/info`. | Zwraca status i odpowiedzialnosci. | istnieje |
| TC-OMS-014-01 | UC-OMS-014 | Order nieterminalny. | `POST /api/orders/{orderId}/cancel`. | Status `Cancelled`, historia dopisana i publikuje `order.cancelled`. | istnieje |
| TC-OMS-014-02 | UC-OMS-014 | Order terminalny. | `POST /api/orders/{orderId}/cancel`. | Zwraca conflict bez zmiany statusu. | do dodania |
| TC-OMS-015-01 | UC-OMS-015 | Order po wydaniu. | Handler `order.completed`. | Status `Completed`, historia dopisana, brak cofania po powtorce. | istnieje |

## Inventory Service

| TC ID | UC | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-INV-001-01 | UC-INV-001 | Magazyn ma pozycje. | `GET /api/admin/inventory/items`. | Zwraca ilosci on hand, reserved, available i progi. | istnieje |
| TC-INV-001-02 | UC-INV-001 | Brak pozycji. | `GET /api/admin/inventory/items`. | Zwraca pusta liste. | do dodania |
| TC-INV-002-01 | UC-INV-002 | Sa pozycje ponizej progu. | `GET /api/admin/inventory/shortages`. | Zwraca tylko braki i reorder quantity. | istnieje |
| TC-INV-002-02 | UC-INV-002 | Brak brakow. | `GET /api/admin/inventory/shortages`. | Zwraca pusta liste. | do dodania |
| TC-INV-003-01 | UC-INV-003 | Istniejacy skladnik. | `POST /api/admin/inventory/items/{ingredientId}/delivery`. | Zwieksza on hand i loguje dostawe. | istnieje |
| TC-INV-003-02 | UC-INV-003 | Ilosc <= 0. | `POST /api/admin/inventory/items/{ingredientId}/delivery`. | Zwraca validation problem. | istnieje |
| TC-INV-004-01 | UC-INV-004 | Istniejacy skladnik. | `POST /api/admin/inventory/items/{ingredientId}/adjustment`. | Ustawia on hand i min safety level, loguje korekte. | istnieje |
| TC-INV-004-02 | UC-INV-004 | Ujemny stan albo prog. | `POST /api/admin/inventory/items/{ingredientId}/adjustment`. | Zwraca validation problem. | istnieje |
| TC-INV-005-01 | UC-INV-005 | `recipe.changed` z nowymi skladnikami. | Handler. | Tworzy snapshot receptury i pozycje magazynu. | istnieje |
| TC-INV-005-02 | UC-INV-005 | `recipe.changed` dla istniejacej receptury. | Handler. | Zastepuje pozycje snapshotu i aktualizuje dane skladnika. | istnieje |
| TC-INV-006-01 | UC-INV-006 | Wszystkie skladniki dostepne. | `order.placed` handler. | Rezerwuje skladniki i tworzy `StockReservation`. | istnieje |
| TC-INV-006-02 | UC-INV-006 | Brak receptury. | `order.placed` handler. | Tworzy failed reservation z `recipe_missing`. | istnieje |
| TC-INV-006-03 | UC-INV-006 | Brak stanu magazynowego. | `order.placed` handler. | Tworzy failed reservation z `ingredient_unavailable`. | istnieje |
| TC-INV-006-04 | UC-INV-006 | Duplikat eventu dla tego orderu. | `order.placed` handler. | Nie podwaja rezerwacji. | istnieje |
| TC-INV-006-05 | UC-INV-006 | Rownolegle ordery na ten sam skladnik. | `order.placed` handler. | Nie przekracza dostepnego stanu. | istnieje |
| TC-INV-007-01 | UC-INV-007 | Rezerwacja udana. | Publikacja wyniku. | Publikuje `inventory.reserved` z reservation id. | istnieje |
| TC-INV-008-01 | UC-INV-008 | Rezerwacja nieudana. | Publikacja wyniku. | Publikuje `inventory.reservation_failed` z reason code. | istnieje |
| TC-INV-009-01 | UC-INV-009 | Panel magazynu otwarty. | Filtrowanie, dostawa, korekta. | UI odswieza liste i pokazuje zmienione wartosci. | do dodania |
| TC-INV-009-02 | UC-INV-009 | Blad API. | Akcja w panelu. | UI pokazuje komunikat bledu bez utraty stanu. | do dodania |
| TC-INV-010-01 | UC-INV-010 | Serwis uruchomiony. | `GET /`, `GET /api/info`. | Zwraca status i odpowiedzialnosci. | istnieje |
| TC-INV-011-01 | UC-INV-011 | Istnieje rezerwacja orderu. | Handler `order.cancelled`. | Reserved wraca do available, status rezerwacji `Released`, log dopisany. | istnieje |
| TC-INV-012-01 | UC-INV-012 | Istnieje rezerwacja orderu. | Handler `order.completed`. | On hand i reserved maleja, status rezerwacji `Consumed`, log dopisany. | istnieje |

## KDS Service

| TC ID | UC | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-KDS-001-01 | UC-KDS-001 | `station.changed` dla nowej stacji. | Handler. | Tworzy station snapshot. | istnieje |
| TC-KDS-001-02 | UC-KDS-001 | `station.changed` dla istniejacej stacji. | Handler. | Aktualizuje code, name, color i aktywnosc. | istnieje |
| TC-KDS-002-01 | UC-KDS-002 | `product.station_routing_changed` ze stacja. | Handler. | Tworzy albo aktualizuje routing snapshot. | istnieje |
| TC-KDS-002-02 | UC-KDS-002 | `product.station_routing_changed` bez stacji. | Handler. | Usuwa routing snapshot. | istnieje |
| TC-KDS-003-01 | UC-KDS-003 | `order.accepted` z produktami majacymi routing. | Handler. | Tworzy ticket i zadania przypisane do stacji. | istnieje |
| TC-KDS-003-02 | UC-KDS-003 | Produkt bez routingu. | Handler. | Tworzy zadanie `routing missing`, bez publikacji do stacji. | istnieje |
| TC-KDS-003-03 | UC-KDS-003 | Duplikat `order.accepted`. | Handler. | Nie tworzy drugiego ticketu. | istnieje |
| TC-KDS-004-01 | UC-KDS-004 | Sa aktywne stacje. | `GET /api/kitchen/stations`. | Zwraca tylko aktywne stacje posortowane. | do dodania |
| TC-KDS-005-01 | UC-KDS-005 | Stacja ma pending i in progress tasks. | `GET /api/kitchen/stations/{stationId}/tasks`. | Zwraca tylko aktywne zadania tej stacji. | do dodania |
| TC-KDS-005-02 | UC-KDS-005 | Nieznana stacja. | `GET /api/kitchen/stations/{stationId}/tasks`. | Zwraca pusta liste albo 404 zgodnie z kontraktem. | do dodania |
| TC-KDS-006-01 | UC-KDS-006 | Zadanie `Pending`. | `POST /api/kitchen/tasks/{taskId}/start`. | Status `InProgress`, publikuje `item.preparation_started`, wysyla SignalR. | istnieje |
| TC-KDS-006-02 | UC-KDS-006 | Zadanie nie istnieje. | `POST /api/kitchen/tasks/{taskId}/start`. | Zwraca 404. | do dodania |
| TC-KDS-006-03 | UC-KDS-006 | Zadanie routing missing. | `POST /api/kitchen/tasks/{taskId}/start`. | Zwraca conflict. | istnieje |
| TC-KDS-007-01 | UC-KDS-007 | Zadanie `InProgress`. | `POST /api/kitchen/tasks/{taskId}/done`. | Status `Done`, publikuje `item.preparation_completed`, wysyla SignalR. | istnieje |
| TC-KDS-007-02 | UC-KDS-007 | Zadanie `Pending`. | `POST /api/kitchen/tasks/{taskId}/done`. | Zwraca conflict. | istnieje |
| TC-KDS-007-03 | UC-KDS-007 | Event przejscia powtorzony. | Start/done drugi raz. | Nie publikuje drugi raz eventu. | istnieje |
| TC-KDS-008-01 | UC-KDS-008 | Klient SignalR w grupie stacji. | Zmiana zadania. | Wiadomosc trafia tylko do grupy tej stacji. | istnieje |
| TC-KDS-009-01 | UC-KDS-009 | Kitchen app otwarta. | Wybor stacji, start i done zadania. | UI aktualizuje kolumny i cache po SignalR. | do dodania |
| TC-KDS-009-02 | UC-KDS-009 | SignalR offline. | UI pracuje z odswiezaniem HTTP. | Pokazuje status polaczenia i pozwala odswiezyc. | do dodania |
| TC-KDS-010-01 | UC-KDS-010 | Serwis uruchomiony. | `GET /`, `GET /api/info`. | Zwraca status i odpowiedzialnosci. | istnieje |

## Packing Service

| TC ID | UC | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-PACK-001-01 | UC-PACK-001 | `order.accepted` z pozycjami. | Handler. | Tworzy manifest z pozycjami. | istnieje |
| TC-PACK-001-02 | UC-PACK-001 | Duplikat `order.accepted`. | Handler. | Nie tworzy drugiego manifestu. | do dodania |
| TC-PACK-002-01 | UC-PACK-002 | Manifest istnieje, item gotowy. | `item.preparation_completed` handler. | Oznacza pozycje jako gotowa. | istnieje |
| TC-PACK-002-02 | UC-PACK-002 | Duplikat eventu item ready. | Handler. | Nie zwieksza licznika drugi raz. | istnieje |
| TC-PACK-002-03 | UC-PACK-002 | Item ready przychodzi przed manifestem. | Handler, potem `order.accepted`. | Gotowosc zostaje zastosowana po utworzeniu manifestu. | istnieje |
| TC-PACK-003-01 | UC-PACK-003 | Ostatnia pozycja manifestu gotowa. | Handler. | Status manifestu `ReadyForPacking`, publikuje `order.ready_for_packing`. | istnieje |
| TC-PACK-003-02 | UC-PACK-003 | Tylko czesc pozycji gotowa. | Handler. | Nie publikuje `order.ready_for_packing`. | istnieje |
| TC-PACK-004-01 | UC-PACK-004 | Sa manifesty nie wydane. | `GET /api/packing/manifests`. | Zwraca aktywne manifesty z licznikami i statusem opoznienia. | czesciowo |
| TC-PACK-004-02 | UC-PACK-004 | Manifest `Issued`. | `GET /api/packing/manifests`. | Nie zwraca wydanego manifestu. | do dodania |
| TC-PACK-005-01 | UC-PACK-005 | Manifest `ReadyForPacking`, poprawny pickup code. | `POST /api/packing/manifests/{manifestId}/issued`. | Status `Issued`, zwraca manifest, wysyla SignalR. | istnieje |
| TC-PACK-005-02 | UC-PACK-005 | Manifest niegotowy albo zly pickup code. | `POST /api/packing/manifests/{manifestId}/issued`. | Zwraca conflict. | do dodania |
| TC-PACK-005-03 | UC-PACK-005 | Nieznany manifest. | `POST /api/packing/manifests/{manifestId}/issued`. | Zwraca 404. | do dodania |
| TC-PACK-006-01 | UC-PACK-006 | Manifest wydany. | Publikacja wyniku. | Publikuje `order.ready_for_pickup` z pickup code i `order.completed`. | istnieje |
| TC-PACK-007-01 | UC-PACK-007 | Klient SignalR terminala podlaczony. | Zmiana manifestu. | Terminal dostaje update manifestu. | do dodania |
| TC-PACK-008-01 | UC-PACK-008 | Packing terminal otwarty. | Lista, refresh, issue. | UI grupuje manifesty i aktualizuje status po akcji. | do dodania |
| TC-PACK-008-02 | UC-PACK-008 | SignalR offline. | Terminal dziala z HTTP refresh. | Pokazuje status polaczenia i pozwala odswiezyc. | do dodania |
| TC-PACK-009-01 | UC-PACK-009 | Serwis uruchomiony. | `GET /`, `GET /api/info`. | Zwraca status i odpowiedzialnosci. | istnieje |

## Przeplywy end-to-end

| TC ID | Flow | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-E2E-001 | FLOW-004 | Demo katalog, receptura, stan magazynu, routing. | Storefront checkout success, KDS start/done, Packing issue. | Order przechodzi przez kuchnie, pakowanie i pickup. | istnieje |
| TC-E2E-002 | FLOW-004 | Za duza ilosc produktu wzgledem magazynu. | Storefront checkout. | Order odrzucony przez inventory. | istnieje |
| TC-E2E-003 | FLOW-001 | Obca marka. | Pobranie menu storefront. | Nie ujawnia danych innej marki. | istnieje |
| TC-E2E-004 | FLOW-002 | Manager zmienia recepture w Catalog. | Upsert recipe. | Inventory odbiera `recipe.changed` i aktualizuje snapshot. | istnieje |
| TC-E2E-005 | FLOW-005 | Mock delivery order. | Webhook mock delivery, potem caly lifecycle. | Order trafia do inventory, KDS, packing. | do dodania |
| TC-E2E-006 | FLOW-006 | Zadanie KDS zakonczone. | `done` na ostatniej pozycji. | Packing publikuje `order.ready_for_packing`, OMS aktualizuje status. | istnieje |
| TC-E2E-007 | FLOW-007 | Manifest gotowy. | Issue manifest z pickup code. | OMS widzi `Completed`. | istnieje |

## Kontrakty i testy techniczne

| TC ID | Obszar | Warunek | Akcja | Oczekiwany wynik | Automatyzacja |
| --- | --- | --- | --- | --- | --- |
| TC-CON-001 | Event contracts | Lista znanych typow zdarzen. | Test kontraktow. | Typy sa unikalne i stabilne. | istnieje |
| TC-CON-002 | Event contracts | Wszystkie znane zdarzenia. | Serializacja envelope. | JSON camelCase, wersja schematu stabilna. | istnieje |
| TC-CON-003 | Event topology | Lista subskrypcji. | Test topologii. | Kolejki i exchange maja stabilne nazwy. | istnieje |
| TC-CON-004 | Architecture | Projekty serwisow. | Test architektury. | Warstwy i referencje zgodne z reguly repo. | istnieje |
| TC-CON-005 | Architecture | Endpoint handlers. | Test architektury. | Kazdy endpoint handler ma test integracyjny. | istnieje |

## Priorytet dopisania brakujacych testow

| Priorytet | Testy |
| --- | --- |
| P1 | Brakujace negatywne testy auth dla Inventory, KDS i Packing: brak naglowka `X-DarkKitchen-Role` oraz zla rola. |
| P1 | Dodatkowe testy `Storefront` handlerow `inventory.reservation_failed` i `item.preparation_started`. |
| P2 | UI E2E dla `admin-panel`, `storefront`, `inventory-panel`, `kitchen-app`, `packing-terminal`. |
| P2 | Idempotencja eventow w OMS, KDS i Packing tam, gdzie obecnie jest tylko czesciowe pokrycie. |
| P3 | Edge cases uploadow assetow i pustych list w query endpointach. |
