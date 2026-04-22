# Dokumentacja Domeny: Inventory Service

## 1. Rola Serwisu

Inventory Service to strażnik stanów magazynowych. Jego głównym zadaniem jest zarządzanie dostępnością surowców (składników) w czasie rzeczywistym oraz zapewnienie, że system nie przyjmie do realizacji zamówienia, którego nie można skompletować z powodu braków na półkach.

## 2. Główne Odpowiedzialności

### Zarządzanie Stanami Magazynowymi (Stock Tracking)

- **Ewidencja ilościowa:** Śledzenie dokładnej liczby/wagi/objętości każdego składnika (np. 50 kg mąki, 200 bułek, 10 litrów sosu).
- **Obsługa Dostaw (Replenishment):** Przyjmowanie informacji o nowych dostawach i zwiększanie stanów magazynowych.
- **Korekty Inwentaryzacyjne:** Możliwość ręcznego skorygowania stanów (np. w przypadku zepsucia się towaru lub błędu w liczeniu).

### Alokacja i Rezerwacja (Stock Allocation)

- **Rezerwacja pod zamówienie:** W momencie wpłynięcia zamówienia serwis "blokuje" potrzebne składniki, aby nie zostały one sprzedane innemu klientowi w tym samym czasie.
- **Zwalnianie rezerwacji:** Jeśli zamówienie zostanie anulowane, składniki wracają do puli dostępnych towarów.
- **Fizyczne zdjęcie ze stanu:** Przekształcenie rezerwacji w trwałe odjęcie ilości po potwierdzeniu przygotowania posiłku.

### Udział w Sadze Zamówienia (Saga Participant)

- **Walidacja dostępności:** Reagowanie na zdarzenie `OrderAccepted`. Na podstawie lokalnej kopii receptur (z Catalog Service) sprawdzane jest, czy suma dostępnych składników wystarczy na pokrycie zamówienia.
- **Emisja wyników:**
  - `InventoryReserved` – jeśli wszystko jest OK (pozwala to na dalszy proces w KDS).
  - `InventoryReservationFailed` – jeśli brakuje choćby jednego elementu (uruchamia proces anulowania zamówienia).

### Alerty i Progi Krytyczne (Safety Stock)

- **Monitoring stanów minimalnych:** Definiowanie progu, poniżej którego system wysyła alert "Kończy się towar".
- **Prognozowanie braków:** (Opcjonalnie) Analiza tempa schodzenia towaru w celu sugerowania zamówień u dostawców.

---

## 🔄 Relacje z innymi serwisami

| Przychodzące Zdarzenie / Akcja            | Reakcja Inventory Service                                                                                                     |
| :---------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------- |
| **`RecipeCreated / Updated` (z Catalog)** | Serwis aktualizuje swoją mapę mapowania: `Danie X = [Składnik A, Składnik B]`.                                                |
| **`OrderAccepted` (z Order Service)**     | Start procedury sprawdzania stanów. Jeśli OK -> rezerwacja i `InventoryReserved`.                                             |
| **`ItemPreparationFailed` (z KDS)**       | (Opcjonalnie) Jeśli kucharz przypalił burgera i musi zrobić go od nowa, Inventory musi zdjąć dodatkową porcję mięsa ze stanu. |

---

## 📊 Mechanizm "Read Model" w Inventory

Aby zachować autonomię, Inventory Service nie pyta Catalog Service o przepis przy każdym zamówieniu. Przechowuje on własną, uproszczoną tabelę mapowania:

- **ProduktID 101** (np. Cheeseburger) wymaga:
  - Składnik ID 50 (Bułka): 1 szt.
  - Składnik ID 51 (Mięso): 100 g.
  - Składnik ID 52 (Ser): 1 szt.

---

## 🗄️ Proponowany Model Danych (Inventory DB)

- **WarehouseItem (Składnik):** `Id, Name, CurrentQuantity, Unit (g/ml/pcs), MinSafetyLevel`
- **StockReservation:** `Id, OrderId, IngredientId, Quantity, ExpiryTime`
- **RecipeSnapshot (Read Model):** `ProductId, IngredientId, QuantityRequired`
- **InventoryLog:** `Id, IngredientId, ChangeType (Delivery/Sale/Waste), Amount, Timestamp`
