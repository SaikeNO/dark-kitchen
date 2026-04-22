# Dokumentacja Domeny: Order Management Service

## 1. Rola Serwisu

Order Management Service (OMS) pełni funkcję centralnego hubu i orkiestratora cyklu życia zamówienia. Jego głównym zadaniem jest przyjmowanie zamówień z różnych źródeł, ich normalizacja (ujednolicenie formatu) oraz nadzorowanie przepływu zamówienia przez kolejne etapy (magazyn, kuchnia, pakowanie).

## 2. Główne Odpowiedzialności

### Ingestion & Normalization (Przyjmowanie i Mapowanie)

- **Adaptery zewnętrzne:** Odbieranie webhooków z Glovo, Uber Eats czy Pyszne.pl i tłumaczenie ich różnorodnych formatów JSON na jeden wspólny standard wewnętrzny.
- **Integracja ze Storefrontem:** Przyjmowanie zamówień z Twoich własnych sklepów (poprzez API).
- **Wstępna walidacja:** Sprawdzanie, czy zamówienie posiada wszystkie wymagane pola (np. adres, ID produktów, kwotę).

### Zarządzanie Stanem Zamówienia (State Machine)

- **Śledzenie statusu:** Zarządzanie globalnym statusem zamówienia (np. `Placed`, `Accepted`, `Preparing`, `ReadyForPickup`, `Completed`, `Cancelled`).
- **Zarządzanie czasem:** Rejestrowanie kluczowych momentów (timestampów) dla celów analitycznych (np. jak długo trwało akceptowanie, a jak długo pakowanie).

### Orkiestracja Sagi (Saga Coordinator)

- **Inicjacja procesu:** Po otrzymaniu zamówienia OMS publikuje zdarzenie `OrderPlaced`, które "budzi" Inventory Service.
- **Obsługa decyzji:** Na podstawie odpowiedzi z magazynu (`InventoryReserved` lub `Failed`) decyduje o dalszych losach zamówienia.
- **Logika kompensacyjna:** Jeśli cokolwiek pójdzie nie tak na późniejszym etapie (np. błąd płatności lub brak towaru), OMS odpowiada za powiadomienie klienta/platformy zewnętrznej o anulowaniu zamówienia.

### Komunikacja z Klientem / Platformami

- **Aktualizacja statusów:** Wysyłanie informacji zwrotnej do Glovo/Uber Eats (np. "Restauracja zaakceptowała zamówienie").

---

## 🔄 Przepływ Statusów (State Machine)

| Status        | Wyzwalacz (Zdarzenie)        | Co się dzieje w systemie?                                         |
| :------------ | :--------------------------- | :---------------------------------------------------------------- |
| **Placed**    | Webhook z Glovo / API Sklepu | Zamówienie zapisane w bazie OMS, wysłany sygnał do Magazynu.      |
| **Accepted**  | `InventoryReserved`          | Magazyn potwierdził składniki. OMS wysyła sygnał do KDS (Kuchni). |
| **Preparing** | `ItemStarted` (z KDS)        | Kucharz zaczął pracę. OMS aktualizuje status dla klienta.         |
| **Ready**     | `OrderReadyForPacking`       | Wszystkie elementy gotowe. Czekamy na spakowanie.                 |
| **Completed** | `OrderPickedUp`              | Kurier odebrał torbę. Zamówienie zamknięte.                       |
| **Rejected**  | `InventoryFailed`            | Brak składników. OMS anuluje zamówienie i powiadamia źródło.      |

---

## 🗄️ Proponowany Model Danych (Order DB)

- **Order Header:** `Id, ExternalId (np. z Glovo), BrandId, Source (Glovo/Store), Status, TotalPrice, CreatedAt, UpdatedAt`
- **Order Items:** `Id, OrderId, ProductId, Name, Quantity, UnitPrice`
- **Order History:** `Id, OrderId, StatusFrom, StatusTo, Timestamp, Reason`
- **Customer Info Snapshot:** `OrderId, Phone, DeliveryNote (bez RODO w core, tylko niezbędne dane do realizacji)`
