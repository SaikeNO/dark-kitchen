# ADR 008: Centralizacja konfiguracji marek i menu w Catalog Service

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Specyfika Dark Kitchen polega na obsłudze wielu marek wirtualnych w ramach jednej fizycznej lokalizacji. System wymaga interfejsu administracyjnego, który pozwoli menedżerowi definiować, jakie marki są aktualnie aktywne, jakie dania oferują oraz z jakich składników (BOM) te dania się składają. Rozważaliśmy stworzenie osobnego serwisu "Admin Service", ale uznano, że byłoby to sztuczne rozdzielenie danych od logiki ich przetwarzania.

## 3. Decyzja

Decydujemy się na rozbudowę **Catalog & Recipe Service** o moduł administracyjny (Back-office API).
Główne funkcjonalności:

1. **Multi-branding:** Wprowadzenie encji `Brand`, która grupuje pozycje menu.
2. **Zarządzanie Recepturami:** Możliwość definiowania składników dla każdego dania, co jest niezbędne dla `Inventory Service`.
3. **Routing Kuchenny:** Przypisywanie dań do stacji (np. "Frytkownica 1"), co konsumuje `KDS Service`.

Użytkownik (Manager) będzie łączył się z tym serwisem poprzez dedykowany frontend (Admin Panel).

## 4. Uzasadnienie

- **Single Source of Truth:** Katalog jest naturalnym właścicielem definicji produktu. Trzymanie marek w tym samym miejscu upraszcza walidację (np. czy marka nie próbuje sprzedać dania, które nie ma przypisanej receptury).
- **Spójność zdarzeń:** Każda zmiana w panelu admina (np. zmiana składu burgera) automatycznie generuje odpowiednie zdarzenia dla Magazynu i Zamówień z jednego miejsca.
- **Uproszczona infrastruktura:** Panel Admina to po prostu dodatkowy zestaw endpointów (zabezpieczonych uprawnieniami administratora) w ramach istniejącego serwisu.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Łatwe zarządzanie wieloma wirtualnymi restauracjami z jednego miejsca.
- Spójna logika BOM (Bill of Materials) – zmiany w recepturach natychmiast propagują się do Magazynu.

### Negatywne (Wyzwania i ryzyka):

- **Bezpieczeństwo:** Konieczność rygorystycznego oddzielenia publicznego API (dla Sklepu) od administracyjnego API (dla Managera) w ramach jednego serwisu (np. poprzez różne polityki autoryzacji).
- **Złożoność UI:** Panel admina dla Dark Kitchen musi obsługiwać złożone relacje (marka -> menu -> danie -> receptura -> stacja).
