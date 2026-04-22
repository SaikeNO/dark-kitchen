# ADR 003: Asynchroniczna walidacja zamówień (Event-Carried State Transfer & Wzorzec Saga)

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Kluczowym momentem w systemie Dark Kitchen jest przyjęcie zamówienia od klienta (Order Service). Zanim zamówienie zostanie przekazane do przygotowania na kuchni (KDS Service), system musi zweryfikować dwie rzeczy:

1. Czy zamawiane pozycje (np. dany rodzaj burgera) faktycznie istnieją w menu i mają aktualną cenę (domena Catalog Service).
2. Czy na magazynie znajduje się wystarczająca ilość półproduktów, aby to zamówienie zrealizować (domena Inventory Service).

Rozważaliśmy synchroniczne odpytywanie tych serwisów przez REST API/gRPC w momencie wpadania zamówienia. Zostało to jednak odrzucone, ponieważ tworzyłoby to silne powiązanie (tight coupling). Awaria Serwisu Magazynowego lub Serwisu Menu skutkowałaby całkowitą niemożnością przyjmowania zamówień, co drastycznie obniżyłoby dostępność (SLA) całego systemu.

## 3. Decyzja

Decydujemy się na całkowicie asynchroniczny przepływ zamówień oparty na dwóch architektonicznych wzorcach:

1. **Event-Carried State Transfer (Replikacja Danych):** Order Service będzie utrzymywał własną, zoptymalizowaną lokalną kopię danych o menu (Read Model). Kopia ta będzie aktualizowana na bieżąco poprzez nasłuchiwanie zdarzeń (np. `MenuItemCreated`, `MenuItemPriceChanged`, `MenuItemDeactivated`) z Serwisu Menu.
2. **Wzorzec Saga (Choreografia):** Sprawdzenie i rezerwacja stanów magazynowych będzie odbywać się asynchronicznie już po wstępnym zaakceptowaniu zamówienia.

## 4. Uzasadnienie

- **Maksymalna dostępność i szybkość:** Kiedy klient klika "Zamów", Order Service waliduje istnienie produktu wyłącznie na podstawie swojej lokalnej bazy danych. Odpowiedź do klienta jest natychmiastowa, a system działa nawet wtedy, gdy Catalog Service i Inventory Service są tymczasowo niedostępne (są "w dole").
- **Prawdziwa autonomia:** Mikroserwisy nie dzielą bazy danych ani nie polegają na sobie w czasie rzeczywistym. Komunikują się wyłącznie za pomocą zdarzeń przesyłanych przez brokera wiadomości (np. RabbitMQ).
- **Zgodność z domeną biznesową:** W rzeczywistości fizycznej kuchnia często przyjmuje zamówienie, po czym po chwili orientuje się, że zabrakło składnika i musi anulować pozycję. Wzorzec Saga idealnie modeluje ten proces kompensacji.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Odporność na awarie (Resilience) – brak efektu domina w przypadku błędu jednego z serwisów.
- Ekstremalnie niski czas odpowiedzi (Low Latency) dla endpointu przyjmującego zamówienia.
- Dobra podstawa do nauki zaawansowanych wzorców projektowych w architekturze Event-Driven.

### Negatywne (Wyzwania i ryzyka):

- **Spójność Ostateczna (Eventual Consistency):** Istnieje niewielkie okno czasowe (np. ułamki sekund), w którym pozycje zostały wycofane z menu, ale Order Service nie przetworzył jeszcze zdarzenia i przyjmie zamówienie na nieistniejący produkt.
- **Konieczność implementacji logiki kompensacyjnej:** Skoro rezerwacja magazynu następuje po przyjęciu zamówienia, musimy zaimplementować tzw. akcje kompensacyjne. Jeśli Order Service wyemituje `OrderCreated`, a Inventory Service odpowie zdarzeniem `InventoryReservationFailed` (brak towaru), Order Service musi potrafić zareagować: zmienić status zamówienia na `REJECTED` i zainicjować proces zwrotu pieniędzy klientowi.
- **Duplikacja danych:** Order Service musi przechowywać podzbiór danych należących do Catalog Service, co wiąże się z minimalnie większym zużyciem zasobów bazodanowych.
