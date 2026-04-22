# ADR 010: Identyfikacja i rejestracja stacji roboczych (Kitchen Stations)

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Każda instancja aplikacji `KitchenApp` musi otrzymywać tylko te zadania, które są przeznaczone dla konkretnej stacji roboczej (np. Grill). System musi umożliwiać łatwe przypisanie fizycznego urządzenia (tabletu) do logicznej stacji zdefiniowanej w katalogu.

## 3. Decyzja

Decydujemy się na hybrydowe podejście:

1. **Definicja stacji** odbywa się w `Catalog Service` (Master Data).
2. **Powiązanie stacji z aplikacją** odbywa się w momencie uruchomienia `KitchenApp` poprzez **proces wyboru/logowania**.
3. **Komunikacja real-time** wykorzystuje mechanizm **SignalR Groups**. Każda stacja tworzy oddzielną grupę subskrypcyjną na serwerze `KDS Service`.

## 4. Uzasadnienie

- **Elastyczność:** Jeśli tablet na Grillu się zepsuje, kucharz może wziąć dowolny inny tablet, zalogować się i wybrać "GRILL", natychmiast wracając do pracy.
- **Skalowalność:** Możemy mieć wiele tabletów przypisanych do tej samej stacji (np. dwóch kucharzy na jednej wielkiej sekcji grillowej) – obaj będą widzieć te same zadania dzięki grupom SignalR.
- **Separacja:** `Catalog Service` trzyma definicje (co to jest Grill), a `KDS Service` zarządza aktywnymi połączeniami (kto teraz pracuje na Grillu).

## 5. Konsekwencje

- **Konieczność zarządzania sesją:** System musi pamiętać wybór stacji po odświeżeniu przeglądarki (użycie `LocalStorage` lub `Cookies`).
- **Synchronizacja:** Przy zmianie mapowania w Katalogu (np. zmiana nazwy stacji), `KDS Service` musi zaktualizować swoje grupy subskrypcyjne.
