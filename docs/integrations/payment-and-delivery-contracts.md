# Kontrakty integracji płatności i delivery

Ten dokument opisuje granice mock integracji z TASK 009. Mocki są celowo deterministyczne i nie zastępują produkcyjnych providerów.

## Storefront payment

Endpoint checkout pozostaje publicznym kontraktem sklepu:

`POST /api/storefront/checkout?brandId={brandId}`

```json
{
  "cartId": "00000000-0000-0000-0000-000000000000",
  "customer": {
    "displayName": "Jan Kowalski",
    "phone": "500600700",
    "deliveryNote": "Kod 12"
  },
  "mockPaymentResult": "success"
}
```

`mockPaymentResult` jest polem testowym. Wartości `failed` i `fail` kończą płatność statusem `Failed` i powodem `mock_payment_failed`. Każda inna wartość, także `null`, oznacza sukces.

Wewnętrzny kontrakt `IPaymentProvider` rozdziela inicjację i potwierdzenie płatności. Produkcyjny provider powinien uzupełnić co najmniej:

- zewnętrzny identyfikator transakcji,
- kwotę i walutę,
- status autoryzacji lub potwierdzenia,
- kod błędu czytelny dla obsługi,
- identyfikatory korelacyjne providerów.

## Mock delivery webhook

Mock endpoint OMS:

`POST /api/mock-delivery/webhooks/orders`

```json
{
  "platform": "glovo",
  "brandId": "00000000-0000-0000-0000-000000000000",
  "externalOrderId": "glovo-123",
  "externalStatus": "created",
  "customer": {
    "displayName": "Jan Kowalski",
    "phone": "500300400",
    "deliveryNote": "Odbiór przez kuriera"
  },
  "items": [
    {
      "menuItemId": "00000000-0000-0000-0000-000000000000",
      "quantity": 1
    }
  ]
}
```

Obsługiwane platformy mock: `glovo`, `uber`, `pyszne`. Nazwa platformy jest normalizowana do małych liter, a `SourceChannel` w OMS ma format `mock-delivery:{platform}`.

`externalStatus` jest opcjonalny. W MVP tylko `created`, `placed` lub brak statusu tworzą zamówienie. Inne statusy są odrzucane walidacją, bo produkcyjne aktualizacje statusu są poza zakresem.

Idempotencja działa po `BrandId + SourceChannel + ExternalOrderId`. Powtórzony webhook zwraca istniejące zamówienie i nie tworzy duplikatu.

## Produkcja poza mockiem

Przyszła integracja produkcyjna musi dodać:

- weryfikację podpisu lub inny mechanizm zaufania webhooka,
- mapowanie natywnych payloadów Glovo/Uber/Pyszne do `IDeliveryOrderAdapter`,
- pełne mapowanie statusów zewnętrznych do procesu OMS,
- obsługę błędów zwrotnych, anulowań i callbacków do platform,
- bezpieczne przechowywanie sekretów poza repozytorium.
