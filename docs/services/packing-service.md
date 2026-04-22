# Dokumentacja Domeny: Packing Service (Wydawka)

## 1. Rola Serwisu

Packing Service odpowiada za końcowy etap operacyjny – agregację wszystkich przygotowanych elementów zamówienia, ich weryfikację oraz przygotowanie do wydania kurierowi. Jest to punkt styku między "produkcją" (KDS) a "logistyką" (Order Management).

## 2. Główne Odpowiedzialności

### Zarządzanie Manifestem (Lista Kontrolna)

- **Budowa Manifestu:** Na podstawie zdarzenia `OrderAccepted` serwis tworzy lokalną listę kontrolną wszystkich elementów, które muszą znaleźć się w torbie (np. 1x Burger, 1x Frytki, 1x Cola).
- **Śledzenie Kompletacji:** Rejestrowanie napływających zdarzeń `ItemPreparationCompleted` i przypisywanie ich do odpowiedniego zamówienia (Correlation ID).

### Obsługa Interfejsu Pakowacza

- **Wizualizacja stanu:** Wyświetlanie na ekranie wydawki statusu zamówień w formacie: "Zamówienie #123: Burger [GOTOWE], Frytki [CZEKAM], Cola [GOTOWE]".
- **Powiadomienie o gotowości:** Gdy wszystkie elementy osiągną status "Gotowe", serwis wysyła sygnał: "MOŻNA PAKOWAĆ" (Order Ready for Packing).

### Obsługa Etykiet i Identyfikacji

- **Generowanie etykiet:** (Opcjonalnie) Integracja z drukarkami termicznymi, aby wydrukować naklejkę z numerem zamówienia i marką (np. logo Burger Ghost).
- **Weryfikacja Marki:** Podpowiadanie pracownikowi, jakiego opakowania powinien użyć (ze względu na model multi-brand).

### Finalizacja i Wydanie

- **Zamykanie procesu:** Po fizycznym spakowaniu i kliknięciu "Wydane", serwis publikuje zdarzenie `OrderReadyForPickup`, co informuje `Order Management Service`, że można wezwać kuriera lub zmienić status dla klienta.

---

## 🔄 Przepływ Agregacji (Aggregation Flow)

1. **Inicjacja:** Otrzymanie `OrderAccepted` -> Utworzenie rekordu "Oczekiwanie na 3 elementy".
2. **Nasłuchiwanie:** - Wpada `ItemCompleted` (Frytki) -> Status: 1/3.
   - Wpada `ItemCompleted` (Burger) -> Status: 2/3.
   - Wpada `ItemCompleted` (Napój) -> Status: 3/3.
3. **Alert:** Aktywacja statusu "Complete" na ekranie pakowania.
4. **Akcja:** Pracownik pakuje torbę, klika przycisk na ekranie.
5. **Koniec:** Emisja zdarzenia `OrderReadyForPickup`.

---

## 🗄️ Proponowany Model Danych (Packing DB)

- **PackingManifest:** `Id, OrderId, BrandId, TotalItemsCount, ReadyItemsCount, Status (Waiting/Full/Packed)`.
- **ManifestItem:** `Id, ManifestId, ItemName, IsReady (Boolean), CompletedAt`.
- **PackagingInstructions (Read-Model):** `BrandId, LogoUrl, BagType (Small/Medium/Large)`.

---

## 🛠️ Wyzwania Techniczne

- **Problem zagubionych zdarzeń:** Co jeśli kucharz nie "bumpnie" frytek? Serwis musi mieć timer (Timeout), który po pewnym czasie podświetli zamówienie na czerwono z pytaniem "Gdzie są te frytki?".
- **Agregacja asynchroniczna:** Musisz zapewnić, że baza danych serwisu pakowania jest odporna na sytuację, w której zdarzenie o gotowości elementu przyjdzie szybciej niż samo zamówienie (Race Condition).
