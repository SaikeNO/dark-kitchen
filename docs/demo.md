# Demo MVP

## Uruchomienie

```powershell
npm run demo
```

Komenda startuje AppHost bez trwalych wolumenow i ze stalymi portami frontendow. Adres Aspire Dashboard pojawi sie w konsoli AppHosta.

## Adresy

- Admin Panel: `http://127.0.0.1:5173`
- Inventory Panel: `http://127.0.0.1:5177`
- Storefront: `http://127.0.0.1:5174`
- Kitchen App: `http://127.0.0.1:5175`
- Packing Terminal: `http://127.0.0.1:5176`

## Scenariusz

1. Otworz Storefront i wybierz marke `Burger Ghost`.
2. Dodaj `Classic Smash` do koszyka.
3. W checkout wpisz dane klienta, zostaw `Mock payment` jako `Success` i kliknij `Zamow`.
4. Zapamietaj identyfikator zamowienia z komunikatu `Order ...`.
5. Otworz Kitchen App, wybierz stacje `GRILL`, kliknij `Start`, a potem `Done` na zadaniu tego zamowienia.
6. Otworz Packing Terminal, znajdz manifest zamowienia w kolumnie `Gotowe` i kliknij `Wydane`.
7. W Aspire Dashboard sprawdz trace i logi dla OMS, Inventory, KDS i Packing. Ten sam `X-Correlation-Id` powinien przechodzic przez wywolania HTTP i zdarzenia.

## Dane Demo

- Marka: `Burger Ghost`
- Produkt: `Classic Smash`
- Stacja: `GRILL`
- Skladniki: `Bulka burgerowa`, `Kotlet wolowy`
- Konto Catalog Manager: `manager@darkkitchen.local` / `Demo123!`
- Konto Catalog Operator: `operator@darkkitchen.local` / `Demo123!`
