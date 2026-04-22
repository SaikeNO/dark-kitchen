# ADR 006: Separacja domeny Katalogu (Master Data) od domeny Magazynu (Stock)

## 1. Status

**Zaakceptowany**

## 2. Kontekst

W architekturze systemu dla wirtualnej kuchni (Dark Kitchen) zarządzanie "jedzeniem" jest kluczowym aspektem. Z biznesowego punktu widzenia pojęcie to ma jednak dwa zupełnie różne znaczenia:

1. **Jedzenie jako koncept/oferta:** Nazwa, cena, zdjęcie, kategoria, receptura (z jakich składników się składa). Te dane zmieniają się rzadko, ale są masowo odczytywane przez klientów i aplikacje delivery.
2. **Jedzenie jako fizyczny zasób:** Liczba sztuk bułek w magazynie, kilogramy mięsa w chłodni, proces rezerwacji pod konkretne zamówienie. Te dane zmieniają się nieustannie (z każdą sekundą) i są ściśle powiązane z procesami operacyjnymi zaplecza.

Stworzenie jednego, wspólnego mikroserwisu np. "Food Service" doprowadziłoby do pomieszania tych dwóch odpowiedzialności. Spowodowałoby to powstanie "wąskiego gardła" wydajnościowego, w którym operacje odczytu menu blokowałyby operacje rezerwacji stanów na magazynie, łamiąc zasady Domain-Driven Design (DDD).

## 3. Decyzja

Decydujemy się na ścisłą separację tych dwóch domen i utworzenie dwóch niezależnych mikroserwisów:

- **Catalog & Recipe Service:** Pełniący rolę Master Data. Będzie jedynym źródłem prawdy o tym, co sprzedajemy, za ile i z czego to się składa (BOM - Bill of Materials).
- **Inventory Service:** Pełniący rolę silnika transakcyjnego dla magazynu. Będzie dbał wyłącznie o ilości, rezerwacje i stany minimalne, całkowicie ignorując abstrakcyjne pojęcia takie jak "zdjęcie" czy "kategoria".

Komunikacja między nimi będzie odbywać się asynchronicznie (Event-Driven). Gdy w Katalogu powstanie nowa receptura, serwis ten wyemituje zdarzenie (np. `RecipeCreated`). Inventory Service zasłyszy to zdarzenie i zaktualizuje swoją lokalną kopię danych (Read-Model), aby wiedzieć, jakie składniki zdjąć z magazynu, gdy wpadnie zamówienie na dany produkt.

## 4. Uzasadnienie

- **Separacja odpowiedzialności (Separation of Concerns):** Menedżer zmieniający zdjęcie burgera nie powinien wpływać na działanie magazyniera rezerwującego mięso na bieżące zamówienia.
- **Różne profile obciążenia (Scalability):** Catalog Service to usługa typu Read-Heavy (dużo odczytów, mało zapisów). Inventory Service to usługa typu Write-Heavy (ogromna liczba ciągłych modyfikacji stanów). Rozdzielenie ich pozwala na zastosowanie innych strategii skalowania oraz dobór optymalnych silników bazodanowych (np. Document DB dla Katalogu, Event Sourcing / szybka baza relacyjna dla Magazynu).
- **Wysoka dostępność (High Availability):** Jeśli Catalog Service ulegnie awarii (np. podczas aktualizacji cen), Inventory Service nadal może poprawnie funkcjonować i obsługiwać zamówienia, opierając się na swoim zsynchronizowanym wcześniej Read-Modelu.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Czystość architektoniczna i idealne odzwierciedlenie rzeczywistych procesów biznesowych.
- Zwiększona odporność całego systemu na awarie częściowe.
- Możliwość rozwijania obu domen przez niezależne zespoły programistów.

### Negatywne (Wyzwania i ryzyka):

- **Złożoność integracji (Eventual Consistency):** Jeśli menedżer usunie składnik z receptury w Katalogu, miną ułamki sekund (lub dłużej w przypadku zatoru na kolejce RabbitMQ), zanim Magazyn dowie się o tej zmianie. W tym czasie może zdarzyć się, że Magazyn zdejmie ze stanu składnik według "starej" receptury.
- **Duplikacja danych:** Inventory Service musi przechowywać podzbiór danych z Catalog Service (identyfikatory dań i przypisane do nich identyfikatory półproduktów), co nieznacznie zwiększa stopień skomplikowania infrastruktury danych.
