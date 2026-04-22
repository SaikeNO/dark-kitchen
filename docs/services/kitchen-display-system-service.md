# Dokumentacja Domeny: KDS Service (Kitchen Display System)

## 1. Rola Serwisu

KDS Service odpowiada za operacyjne zarządzanie procesem przygotowania posiłków. Jego zadaniem jest rozbicie zamówienia na konkretne elementy (zadania) i wyświetlenie ich na odpowiednich stanowiskach pracy (stacjach) w kuchni w czasie rzeczywistym.

## 2. Główne Odpowiedzialności

### Dekompozycja Zamówienia (Order Splitting)

- **Rozbijanie na elementy:** Gdy OMS potwierdzi zamówienie, KDS dzieli je na części składowe (np. zamówienie na "Burger Combo" zostaje rozbite na zadanie dla stacji GRILL oraz zadanie dla stacji FRYTOWNICA).
- **Routing na stacje:** Na podstawie danych z _Catalog Service_, KDS wie, który tablet w kuchni powinien wyświetlić dany element.

### Komunikacja w Czasie Rzeczywistym (Real-time UI)

- **Obsługa WebSockets:** KDS musi utrzymywać aktywne połączenia z tabletami kucharzy, aby natychmiast przesyłać nowe zadania bez konieczności odświeżania aplikacji przez personel.
- **Synchronizacja stanów:** Jeśli jeden kucharz na stacji współdzielonej weźmie zadanie do realizacji, stan ten musi natychmiast zaktualizować się na innych tabletach w obrębie tej samej stacji.

### Zarządzanie Cyklem Przygotowania

- **Śledzenie postępu:** Rejestrowanie momentu rozpoczęcia (`ItemStarted`) i zakończenia (`ItemCompleted`) pracy nad każdym elementem.
- **Priorytetyzacja:** Szeregowanie zadań w kolejce (np. według czasu wpadnięcia zamówienia lub czasu potrzebnego na przygotowanie – tzw. _cook time_).

### Agregacja Informacji zwrotnej

- **Powiadamianie systemu:** Emitowanie zdarzeń o statusie produkcji, które są kluczowe dla `Packing Service` (agregacja) oraz `Order Service` (status dla klienta).

---

## 🔄 Przepływ pracy (Kitchen Workflow)

1. **Nasłuch:** KDS odbiera zdarzenie `OrderAccepted`.
2. **Analiza:** Sprawdza w swoim lokalnym Read-Modelu (zsynchronizowanym z Catalog Service), gdzie wysłać poszczególne pozycje.
3. **Publikacja na Tablety:** Wysyła JSON-a przez WebSocket do odpowiednich aplikacji mobilnych.
4. **Interakcja kucharza:** - Kucharz klika "START" -> KDS wysyła `ItemPreparationStarted`.
   - Kucharz klika "DONE" -> KDS wysyła `ItemPreparationCompleted`.

---

## 🗄️ Proponowany Model Danych (KDS DB)

- **KitchenTicket:** `Id, OrderId, BrandId (dla oznaczenia logo na ekranie), Status (Pending/InPrep/Done), CreatedAt`.
- **KitchenTask:** `Id, TicketId, StationId, ItemName, Note (np. "bez cebuli"), Status`.
- **Station (Read-Model):** `Id, Name, AssignedIpAddress / ConnectionId`.

---

## 🛠️ Wyzwania Techniczne

- **Stateful Connections:** Zarządzanie tysiącami połączeń WebSocket (w .NET idealnie nadaje się do tego **SignalR**).
- **Trudne warunki:** Aplikacja mobilna (frontend KDS) musi być odporna na tłuste palce, parę wodną i przypadkowe rozłączenia WiFi (automatyczne reconnecity).
- **Obsługa "Bump":** W gastronomii zakończenie zadania nazywa się "bumpnięciem". KDS musi obsługiwać to błyskawicznie, by nie tworzyć zatorów na kuchni.
