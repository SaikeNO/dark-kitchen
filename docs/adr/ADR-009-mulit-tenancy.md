# ADR 009: Implementacja Multi-tenancy dla wielu marek sprzedażowych

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Model biznesowy zakłada uruchomienie wielu sklepów internetowych (np. Burger Ghost, Sushi Master, Green Salad) działających pod różnymi markami, ale obsługiwanych przez tę samą infrastrukturę backendową. Musimy zdecydować, czy dla każdej marki tworzymy osobne instancje serwisów, czy obsługujemy je w ramach jednej współdzielonej logiki.

## 3. Decyzja

Decydujemy się na podejście **Shared Application, Isolated Data (Multi-tenancy)** w ramach `Storefront Service`.

1.  **Identyfikacja Marki:** Każde żądanie z frontendu będzie niosło ze sobą `BrandId` (wykrywane na podstawie domeny lub klucza API).
2.  **Filtracja Danych:** `Storefront Service` będzie filtrował menu z `Catalog Service` oraz przypisywał `BrandId` do każdego nowo utworzonego zamówienia.
3.  **Frontend White-label:** Tworzymy jedną bazę kodu dla frontendu, która dynamicznie ładuje zasoby (CSS, grafiki, teksty) na podstawie rozpoznanej marki.

## 4. Uzasadnienie

- **Ekonomia skali:** Utrzymanie jednego mikroserwisu obsługującego 10 marek jest znacznie tańsze i prostsze niż utrzymanie 10 osobnych mikroserwisów.
- **Szybkość wdrażania (Time-to-Market):** Dodanie nowej marki sprowadza się do dodania wpisu w bazie danych `Catalog Service` i skonfigurowania nowej domeny dla frontendu.
- **Spójność:** Centralne zarządzanie zamówieniami (`Order Management Service`) otrzymuje ustandaryzowane dane niezależnie od tego, która marka dokonała sprzedaży.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Drastycznie mniejszy narzut na infrastrukturę (mniej kontenerów w .NET Aspire).
- Łatwiejsze raportowanie zbiorcze (widzisz sprzedaż wszystkich marek w jednym miejscu).
- Możliwość tworzenia marek "pop-up" (uruchamianych tylko na weekend).

### Negatywne (Wyzwania i ryzyka):

- **Wyciek danych (Data Leaking):** Ryzyko, że błąd w kodzie spowoduje wyświetlenie zamówienia z marki A klientowi marki B. Wymaga to rygorystycznego testowania filtrów `BrandId`.
- **Współdzielone zasoby:** Jeśli jedna marka zyska nagłą popularność (viral), może spowolnić działanie sklepów pozostałych marek (tzw. "noisy neighbor problem").
