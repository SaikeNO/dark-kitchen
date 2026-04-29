# TASK 009: Integracje zewnętrzne: płatności i delivery

## Cel

Przygotować granice integracyjne dla płatności i platform delivery bez wiązania MVP z konkretnym dostawcą produkcyjnym.

## Zakres

- Interfejs adaptera płatności w Storefront Service.
- Mock payment adapter dla scenariuszy success i failure.
- Interfejs adaptera delivery webhook w OMS.
- Mock delivery webhook dla Glovo/Uber/Pyszne jako ujednolicony format wejściowy.
- Miejsce na mapowanie zewnętrznych statusów do statusów OMS.
- Dokumentacja kontraktów wejściowych adapterów.

## Poza zakresem

- Produkcyjne podpisy webhooków.
- Certyfikacja z operatorami delivery.
- Realne zwroty, chargebacki i rozliczenia prowizji.

## Zależności

- [TASK 005](./005-order-management-service.md)
- [TASK 006](./006-storefront-service-i-sklep.md)
- [ADR 007](../adr/ADR-007-storefront.md)
- [ADR 011](../adr/ADR-011-stos-technologiczny.md)

## Kroki realizacji

1. Zdefiniować kontrakt `PaymentProvider` z metodami inicjacji i potwierdzenia płatności.
2. Zaimplementować mock payment adapter z deterministycznymi wynikami testowymi.
3. Zdefiniować kontrakt `DeliveryOrderAdapter` mapujący zewnętrzne payloady do modelu OMS.
4. Dodać mock webhook endpoint z przykładowymi payloadami dla kanałów delivery.
5. Dodać walidację idempotencji po zewnętrznym `ExternalOrderId`.
6. Udokumentować, które pola są wymagane do realnej integracji w przyszłości.

## Kryteria akceptacji

- Mock payment success i failure są testowalne automatycznie.
- Mock delivery webhook tworzy zamówienie w OMS przez ten sam model co inne kanały.
- Powtórzony webhook nie tworzy duplikatu zamówienia.
- Realny provider może zostać dodany jako nowa implementacja adaptera bez zmiany domeny OMS.
- Dokumentacja jasno oddziela mock od przyszłej integracji produkcyjnej.

## Scenariusze testowe

- Storefront z mock success tworzy zamówienie.
- Storefront z mock failure zapisuje nieudaną transakcję i kończy checkout.
- Mock delivery webhook z tym samym `ExternalOrderId` jest idempotentny.
- Nieznany kanał delivery jest odrzucany czytelnym błędem.
