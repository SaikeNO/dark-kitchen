# ADR 005: Zastosowanie wzorca Event Aggregator w Serwisie Pakowania

## 1. Status

**Zaakceptowany**

## 2. Kontekst

W procesie działania wirtualnej kuchni, zamówienia są dekomponowane na pojedyncze zadania (np. burger, frytki, napój), które są przygotowywane asynchronicznie na różnych, niezależnych stacjach roboczych (obsługiwanych przez KDS Service).

Stanowisko wydawki (obsługiwane przez Packing Service) musi wiedzieć, kiedy wszystkie elementy wchodzące w skład danego zamówienia są fizycznie gotowe, aby rozpocząć proces pakowania i wezwać kuriera. Rozważaliśmy podejście oparte na odpytywaniu (Polling) – Packing Service co kilka sekund wysyłałby zapytanie do KDS Service: _"Czy zamówienie #123 jest już w całości gotowe?"_. Takie podejście generowałoby jednak ogromny, niepotrzebny ruch sieciowy, obciążało bazy danych i łamało zasadę odwrócenia kontroli.

## 3. Decyzja

Decydujemy się na zastosowanie wzorca **Agregatora Zdarzeń (Event Aggregation)** z wykorzystaniem identyfikatora korelacji (Correlation ID).

Serwis Pakowania będzie działał jako autonomiczny stanowy nasłuchiwacz (Stateful Consumer). Na podstawie zdarzenia `OrderAccepted` zbuduje u siebie lokalną "listę kontrolną" (manifest) wymaganych elementów dla danego zamówienia. Następnie będzie nasłuchiwał zdarzeń `ItemPreparationCompleted` płynących z różnych stacji kuchennych, odznaczał pozycje na swojej liście i samodzielnie ewaluował, czy zamówienie jest już kompletne.

## 4. Uzasadnienie

- **Brak odpytywania (No Polling):** System reaguje w czasie rzeczywistym. Eliminujemy zbędny ruch sieciowy między mikroserwisami.
- **Autonomia i luźne powiązanie (Loose Coupling):** Packing Service nie musi wiedzieć, jak działa KDS Service ani z ilu stacji składa się kuchnia. Interesuje go tylko odbieranie zdarzeń z powiązanym `OrderId` (Correlation ID).
- **Odporność na awarie:** Jeśli po usmażeniu frytek i zgłoszeniu tego faktu stacja frytownicy ulegnie awarii (lub zresetuje się jej tablet), Packing Service i tak pamięta, że frytki dla zamówienia #123 są już na wydawce.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Reakcja w czasie zbliżonym do rzeczywistego (Near Real-Time) – powiadomienie o gotowości do pakowania pojawia się w ułamku sekundy po kliknięciu "Gotowe" przez ostatniego kucharza.
- Łatwość w skalowaniu i monitorowaniu "wąskich gardeł" (łatwo policzyć, ile zamówień "wisi" w Packing Service w oczekiwaniu na ostatni element).
- Naturalne odzwierciedlenie procesu biznesowego (zbieranie elementów do koszyka na wydawce).

### Negatywne (Wyzwania i ryzyka):

- **Zarządzanie stanem (Stateful Service):** W przeciwieństwie do prostych bezstanowych funkcji, Packing Service musi posiadać własną bazę danych (lub szybki magazyn w pamięci, np. Redis), aby przechowywać "rozgrzebane", częściowo skompletowane zamówienia.
- **Problem "Osieroconych Zamówień" (Orphaned Orders):** Jeśli kucharz zapomni kliknąć "Gotowe" na swoim tablecie lub zgubi się zdarzenie z sieci, zamówienie w Serwisie Pakowania nigdy nie osiągnie 100%. Wymaga to implementacji mechanizmu **Time-To-Live (TTL)** lub monitora opóźnień (Timeout Manager), który podniesie alert menedżerowi po np. 30 minutach braku aktywności dla otwartego zamówienia.
