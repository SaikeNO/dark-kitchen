# Dokumentacja Domeny: Storefront Service

## 1. Rola Serwisu

Storefront Service pełni rolę bramy wejściowej dla klientów końcowych. Jest to serwis typu **BFF (Backend for Frontend)**, który konsoliduje dane z innych usług, aby dostarczyć je w optymalnej formie do aplikacji webowej lub mobilnej. Zarządza on również tożsamością użytkownika oraz procesem płatności.

## 2. Główne Odpowiedzialności

### Zarządzanie Klientem i Tożsamością (Identity)

- **Rejestracja i Logowanie:** Zarządzanie kontami użytkowników, uwierzytelnianie (np. JWT) oraz autoryzacja.
- **Profile Użytkowników:** Przechowywanie danych kontaktowych, ulubionych adresów dostawy oraz historii zamówień (dla konkretnego klienta).

### Integracja Płatności (Payment Gateway)

- **Procesowanie Transakcji:** Integracja z zewnętrznymi dostawcami płatności (Stripe, PayU, BLIK).
- **Zarządzanie Statusem Płatności:** Oczekiwanie na potwierdzenie z bramki i informowanie `Order Service`, że zamówienie zostało opłacone i może trafić na kuchnię.

### Agregacja Danych (BFF - Backend for Frontend)

- **Prezentacja Menu:** Pobieranie surowych danych z `Catalog Service` i wzbogacanie ich o specyficzne dla klienta informacje (np. czy dany produkt jest dostępny w jego lokalizacji).
- **Multi-brand Logic:** Rozpoznawanie, z której domeny/marki (np. _burgerghost.pl_ czy _veganbowl.pl_) przyszedł klient i serwowanie odpowiedniego zestawu danych (kolory, logo, asortyment).

### Koszyk i Checkout

- **Zarządzanie Koszykiem:** Przechowywanie tymczasowych wyborów klienta przed złożeniem zamówienia.
- **Finalizacja Zamówienia:** Przekazywanie kompletnego koszyka wraz z danymi klienta i statusem płatności do `Order Management Service`.

---

## 🔄 Przepływ Zakupowy (Customer Journey)

1. **Inicjacja:** Klient wchodzi na stronę marki X -> Storefront rozpoznaje markę po domenie.
2. **Przeglądanie:** Storefront pobiera menu dla tej marki z `Catalog Service` i wyświetla je klientowi.
3. **Płatność:** Klient składa zamówienie -> Storefront inicjuje płatność w zewnętrznym systemie.
4. **Potwierdzenie:** Po otrzymaniu potwierdzenia płatności, Storefront wysyła do `Order Service` komunikat: _"Mam nowe opłacone zamówienie dla marki X!"_.

---

## 🗄️ Proponowany Model Danych (Storefront DB)

- **User:** `Id, Email, PasswordHash, DefaultAddressId, PreferredBrandId`.
- **AddressBook:** `Id, UserId, City, Street, PostalCode, Floor/Apartment`.
- **PaymentTransaction:** `Id, OrderId, ExternalTransactionId, Amount, Status (Pending/Success/Failed)`.
- **BrandConfiguration (Read-Model):** `BrandId, ThemeColors, DomainUrl, SupportContact`.

---

## 🛠️ Wyzwania Techniczne

- **Bezpieczeństwo (Security):** Storefront to jedyny serwis (poza Panelami Admina) wystawiony na publiczny ruch. Musi posiadać solidną ochronę przed atakami typu Brute Force czy SQL Injection.
- **Ochrona Danych (RODO):** To tutaj będziesz przechowywać imiona, nazwiska i adresy. Musisz zadbać o szyfrowanie danych wrażliwych.
- **Obsługa sesji:** Efektywne zarządzanie sesjami użytkowników, aby klient nie musiał logować się przy każdym odświeżeniu strony.
