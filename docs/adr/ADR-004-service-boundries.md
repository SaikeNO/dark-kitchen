# ADR 005: Dekompozycja systemu i określenie granic mikroserwisów (Service Boundaries)

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Zgodnie z decyzją podjętą w ADR 001, system będzie oparty na architekturze mikroserwisów. Głównym wyzwaniem projektowym na tym etapie jest odpowiednie podzielenie logiki biznesowej.
Zbyt duża fragmentacja (np. osobny serwis do smażenia frytek i osobny do grillowania mięsa) doprowadzi do stworzenia tzw. "nanoserwisów", co drastycznie skomplikuje komunikację i utrzymanie. Z kolei zbyt słaby podział (np. połączenie zamówień, kuchni i magazynu w jeden serwis) stworzy "rozproszony monolit", niwecząc zalety mikroserwisów. Potrzebujemy podziału opartego na rzeczywistych, fizycznych procesach i domenach występujących w restauracji typu Dark Kitchen.

## 3. Decyzja

Decydujemy się na podział systemu na 5 głównych, autonomicznych mikroserwisów, opartych na niezależnych domenach biznesowych (Bounded Contexts):

1. **Order Management Service:** Odpowiada wyłącznie za interakcję ze światem zewnętrznym (platformy delivery) oraz cykl życia całego zamówienia z perspektywy klienta.
2. **Catalog & Recipe Service:** Pełni rolę słownika centralnego (Master Data). Zarządza ofertą (menu), cenami oraz recepturami (BOM - Bill of Materials).
3. **Inventory Service:** Zajmuje się wyłącznie śledzeniem rzeczywistych stanów magazynowych oraz alokacją półproduktów pod konkretne zamówienia.
4. **KDS (Kitchen Display System) Service:** Odpowiada za operacje na samej kuchni. Zarządza tabletami na stanowiskach roboczych i koordynuje poszczególne zadania dla kucharzy (tzw. Kitchen Tickets).
5. **Packing & Assembly Service:** Domena strefy wydawki. Skupia się wyłącznie na agregacji gotowych elementów i zarządzaniu fizycznym procesem pakowania dla kuriera.

## 4. Uzasadnienie

- **Separacja odpowiedzialności (Separation of Concerns):** Każdy serwis ma jeden jasny powód do zmiany. Zmiana w sposobie obliczania marży dotknie tylko `Catalog Service`, a zmiana w układzie tabletów na stanowiskach dotknie tylko `KDS Service`.
- **Skalowalność asymetryczna:** Ruch w systemie rozkłada się nierównomiernie. W godzinach szczytu `KDS Service` będzie przetwarzał tysiące małych zdarzeń z tabletów co sekundę, podczas gdy `Catalog Service` będzie praktycznie bezczynny (menu zmienia się rzadko). Podział pozwala nam skalować (dokładać instancje) tylko do tych serwisów, które tego w danej chwili potrzebują.
- **Język wszechobecny (Ubiquitous Language):** Nazwy i granice serwisów idealnie odzwierciedlają to, jak ze sobą rozmawiają pracownicy fizycznej kuchni (Menadżer -> Magazynier -> Kucharz -> Pakowacz).

## 5. Konsekwencje

### Pozytywne (Zalety):

- Zminimalizowane ryzyko konfliktów w kodzie – różne komponenty mogą być rozwijane niezależnie.
- Możliwość dobierania optymalnych typów baz danych dla każdego serwisu (np. relacyjny PostgreSQL dla finansów w `Order Service`, a szybki, in-memory Redis dla stanów tabletów w `KDS Service`).

### Negatywne (Wyzwania i ryzyka):

- **Zarządzanie współdzielonymi identyfikatorami:** Skoro zamówienie wędruje przez 4 różne serwisy, musimy bezwzględnie pilnować przekazywania `OrderId` w każdym komunikacie, aby nie zgubić kontekstu biznesowego.
- **Ewolucja API:** Jeśli w przyszłości zmieni się struktura tego, z czego składa się "danie", będzie to wymagało ostrożnej koordynacji aktualizacji kontraktów (schematów zdarzeń) pomiędzy `Catalog Service`, `Inventory Service` a `Order Service`.
