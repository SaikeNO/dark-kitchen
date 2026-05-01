# Standardy projektowe i architektoniczne

Ten dokument opisuje standardy, według których należy rozwijać i reviewować system Dark Kitchen. Może posłużyć jako instrukcja dla agenta code review.

## Priorytety review

1. Zgodność z ADR-ami i granicami domen.
2. Poprawność przepływu zdarzeń, idempotencji i korelacji.
3. Bezpieczeństwo danych klientów, zamówień i płatności.
4. Testowalność zmian oraz czytelne kryteria akceptacji.
5. Prostota implementacji zgodna z edukacyjnym MVP.

## Architektura

- Każdy mikroserwis odpowiada za jeden bounded context opisany w dokumentacji domenowej.
- Serwisy nie współdzielą bazy danych, modeli EF ani tabel domenowych.
- Komunikacja między serwisami powinna być domyślnie asynchroniczna przez RabbitMQ i WolverineFx.
- Synchroniczne HTTP między serwisami jest wyjątkiem i wymaga uzasadnienia w PR.
- AppHost jest jedynym miejscem lokalnej orkiestracji zasobów.
- ServiceDefaults jest jedynym miejscem wspólnej konfiguracji health checków, service discovery, resilience i OpenTelemetry.

## Backend .NET

- Każdy bounded context serwisu .NET ma projekty klas `Api`, `Domain`, `Features` i `Infrastructure`, zgodnie z ADR 014.
- `Api` jest composition rootem. Nie umieszczamy w nim logiki endpointów ani konkretnych use-case’ów.
- Każdy nowy endpoint Minimal API trafia do slice’a w projekcie `Features`; request/response DTO, walidacja i mapping powinny być możliwie blisko tego endpointu.
- Encje domenowe trzymamy w projekcie `Domain`, po jednej encji na plik. Preferujemy pragmatyczne DDD: fabryki i metody zmiany stanu zamiast publicznych setterów.
- `DbContext` może znajdować się w `Features/Application`, bo w tym projekcie jest traktowany jako aplikacyjna abstrakcja dostępu do danych. Provider EF, design-time factory, migracje i inicjalizacja bazy należą do `Infrastructure`.
- Projekty API używają ASP.NET Core Minimal APIs lub kontrolerów tylko wtedy, gdy moduł faktycznie potrzebuje rozbudowanego modelu MVC.
- Endpointy publiczne powinny zwracać stabilne DTO, a nie encje domenowe lub encje EF.
- Walidacja wejścia ma być jawna i blisko granicy API.
- Logi zdarzeń biznesowych muszą zawierać `CorrelationId`, a dla danych multi-brand także `BrandId`.
- Nie wolno wprowadzać globalnych singletonów ze stanem domenowym.
- Konfiguracja sekretów i connection stringów nie może być hardcodowana w kodzie ani w repozytorium.

## Dane

- Każdy serwis ma własną bazę PostgreSQL i własne migracje.
- Inny serwis może przechowywać tylko read-model albo snapshot danych właściciela.
- Zmiany danych i publikacja eventów muszą używać outboxa, gdy skutkują komunikacją międzyserwisową.
- Konsumenci zdarzeń muszą być idempotentni po `EventId` lub równoważnym kluczu biznesowym.
- Operacje zależne od `BrandId` muszą mieć test zapobiegający wyciekowi danych między markami.

## Zdarzenia i kontrakty

- Zdarzenia używają JSON camelCase.
- Minimalna koperta zdarzenia zawiera `EventId`, `EventType`, `OccurredAt`, `CorrelationId`, `CausationId`, `SchemaVersion` i `BrandId`, jeśli dotyczy.
- Zdarzenia są wersjonowane addytywnie. Usuwanie lub zmiana znaczenia pola wymaga nowej wersji kontraktu.
- Handler zdarzenia nie może zakładać dokładnej kolejności dostarczenia, chyba że kolejność jest wymuszona i opisana.
- Retry i dead letter behavior muszą być jawne dla handlerów wykonujących skutki biznesowe.

## Frontend

- Aplikacje UI używają React, TypeScript i Vite.
- UI operacyjne, takie jak KDS i Packing, ma być szybkie, czytelne, odporne na przypadkowe dotknięcia i dobre na tabletach.
- Storefront jest white-label i nie może mieszać stanu między markami.
- Komponenty nie powinny wykonywać logiki domenowej, która należy do backendu.
- Stan klienta musi być odtwarzalny po odświeżeniu strony tam, gdzie wymaga tego proces operacyjny.

## Testy

- Każdy endpoint handler `XEndpoint.cs` musi mieć odpowiadający test integracyjny `XEndpointTests.cs` w projekcie integracyjnym danego serwisu.
- Testy integracyjne backendu traktują API jako black box: używają HTTP i własnych DTO testowych, bez referencji do `Features` ani `Infrastructure`.
- Zmiana kontraktu zdarzenia wymaga testu kontraktowego w `DarkKitchen.ContractTests`.
- Testy kontraktowe dotyczą zdarzeń integracyjnych, a nie snapshotów HTTP.
- Przepływy między serwisami powinny być testowane przez Aspire.Hosting.Testing.
- Testy jednostkowe dodajemy dla nietrywialnych encji domenowych i agregatów z realnymi inwariantami, kalkulacją, przejściami stanu lub idempotencją.
- Krytyczne ścieżki UI powinny mieć test Playwright.
- Brak testu przy zmianie ryzykownej wymaga wyraźnego uzasadnienia w PR.

## Observability

- Nowe serwisy muszą używać ServiceDefaults.
- Health endpointy `/health` i `/alive` muszą działać w każdym API.
- Trace powinien przechodzić przez granice serwisów z tym samym `CorrelationId`.
- Logi nie mogą zawierać haseł, tokenów, pełnych danych kart, ani nadmiarowych danych osobowych.
- Błędy integracyjne powinny mieć czytelny log z nazwą adaptera i identyfikatorem korelacji.

## Bezpieczeństwo

- Dane osobowe i płatnicze są minimalizowane w usługach, które ich nie potrzebują.
- Storefront i panele administracyjne muszą mieć osobne polityki autoryzacji, gdy pojawi się identity.
- Endpointy administracyjne nie mogą być dostępne przez publiczny storefront.
- Webhooki zewnętrzne muszą docelowo weryfikować podpis lub równoważny mechanizm zaufania.
- Dane wejściowe z platform zewnętrznych są traktowane jako niezaufane.

## Kryteria blokujące review

Agent code review powinien blokować zmianę, jeśli:

- łamie granice serwisów lub wprowadza współdzieloną bazę danych,
- publikuje event bez outboxa po zapisie domenowym,
- handler eventu nie jest idempotentny,
- pomija `CorrelationId` w przepływie międzyserwisowym,
- miesza dane różnych marek bez testu izolacji `BrandId`,
- hardcoduje sekret, connection string lub token,
- usuwa testy bez równoważnego pokrycia,
- ignoruje zaakceptowane ADR-y bez nowej decyzji architektonicznej.
