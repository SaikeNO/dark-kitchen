# Dokumentacja Domeny: Catalog & Recipe Service (Back-office)

## 1. Catalog & Recipe Service (Mikroserwis)

Ten serwis pełni rolę **Master Data Management (MDM)**. Jest jedynym źródłem prawdy (Single Source of Truth) o tym, co restauracja oferuje, pod jakimi markami i w jaki sposób techniczny te dania mają zostać przygotowane.

### Główne Odpowiedzialności:

- **Zarządzanie Markami (Multi-brand):** Definiowanie wirtualnych marek (np. "Burger Ghost", "Sushi Master"). Przechowywanie metadanych marek (logo, opisy, unikalne identyfikatory).
- **Zarządzanie Menu (Katalog):** Strukturyzowanie oferty w kategorie i produkty. Przypisywanie produktów do konkretnych marek.
- **Zarządzanie Recepturami (BOM - Bill of Materials):** Definiowanie dokładnego składu każdego dania (np. 1x bułka, 200g mięsa, 2x plastry sera). Dane te są kluczowe dla `Inventory Service`.
- **Routing Kuchenny:** Określanie, na które stanowisko (stację) w kuchni ma trafić dany element zamówienia (np. frytki -> FRYTOWNICA, burger -> GRILL).
- **Zarządzanie Cennikami:** Przechowywanie cen podstawowych produktów. Możliwość różnicowania cen tego samego produktu w zależności od marki (np. te same frytki mogą mieć inną cenę w marce "Premium Burger" i "Street Food").
- **Publikacja Zdarzeń (Events):** Informowanie reszty systemu o zmianach:
  - `MenuItemCreated / Updated` -> Informacja dla Sklepu i Order Service.
  - `RecipeDefined / Changed` -> Informacja dla Inventory Service (ile surowców schodzi).
  - `PriceChanged` -> Informacja dla systemów sprzedażowych.

---

## 2. Panel Manadżera (Aplikacja Webowa)

Interfejs graficzny (Back-office) przeznaczony dla właścicieli i menedżerów kuchni. Pozwala na bezkodowe zarządzanie całą logiką biznesową zaszytą w `Catalog Service`.

### Główne Funkcjonalności:

- **Dashboard Zarządzania Markami:** Moduł do dodawania nowych wirtualnych marek i aktywowania/dezaktywowania ich w czasie rzeczywistym.
- **Edytor Menu i Produktów:** Kreator typu "przeciągnij i upuść" do budowania struktury menu, wgrywania zdjęć i edycji opisów marketingowych.
- **Konfigurator Receptur:** Zaawansowany moduł, w którym menedżer łączy produkty z menu z konkretnymi półproduktami (surowcami) z magazynu.
- **Zarządzanie Stacjami (Kitchen Setup):** Definiowanie fizycznego układu kuchni – tworzenie stacji roboczych i przypisywanie do nich kategorii dań.
- **Moduł Uprawnień:** Autoryzacja i uwierzytelnianie (Identity) dla pracowników administracyjnych.

---

## 🔄 Relacje z innymi serwisami (Przykład)

| Akcja w Catalog Service                                     | Reakcja innego serwisu                                                                                            |
| :---------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------- |
| **Menedżer dodaje bekon do receptury burgera.**             | `Inventory Service` od teraz będzie rezerwował dodatkowo jedną porcję bekonu przy każdym zamówieniu tego burgera. |
| **Menedżer zmienia cenę pizzy.**                            | `Storefront Service` (Sklep) natychmiast wyświetla nową cenę klientowi.                                           |
| **Menedżer przenosi steki ze stacji GRILL na stację PIEC.** | `KDS Service` zacznie wyświetlać nowe zamówienia na steki na innym tablecie w kuchni.                             |

---

## 🗄️ Proponowany Model Danych (Uproszczony)

- **Brand:** `Id, Name, LogoUrl, IsActive`
- **Product:** `Id, BrandId, Name, Description, BasePrice, CategoryId`
- **Ingredient (Półprodukt):** `Id, Name, Unit (g, ml, szt)`
- **Recipe (BOM):** `ProductId, IngredientId, Quantity`
- **Station:** `Id, Name, DisplayColor`
