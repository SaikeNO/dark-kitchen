# TASK 001: Fundamenty rozwiązania i Aspire

## Cel

Utworzyć techniczny fundament repozytorium: solution .NET, AppHost, ServiceDefaults, projekty mikroserwisów, projekty frontendowe i lokalną orkiestrację zgodną z [ADR 011](../adr/ADR-011-stos-technologiczny.md).

## Zakres

- Utworzenie głównego solution dla backendu.
- Dodanie projektów AppHost i ServiceDefaults .NET Aspire.
- Dodanie pustych projektów API dla: Catalog, Inventory, Order Management, Storefront, KDS i Packing.
- Dodanie aplikacji React + TypeScript + Vite dla: Admin Panel, Storefront, Kitchen App i Packing Terminal.
- Podłączenie PostgreSQL, RabbitMQ i opcjonalnego Redis jako zasobów Aspire.
- Dodanie standardowego health endpointu i OpenTelemetry przez ServiceDefaults.

## Poza zakresem

- Implementacja logiki domenowej.
- Realne integracje płatności i delivery.
- Produkcyjne manifesty Kubernetes lub Azure Container Apps.

## Zależności

- [ADR 001](../adr/ADR-001-architecture.md)
- [ADR 002](../adr/ADR-002-aspire.md)
- [ADR 011](../adr/ADR-011-stos-technologiczny.md)

## Kroki realizacji

1. Utworzyć solution i ustalić konwencję nazw projektów.
2. Dodać AppHost z zasobami PostgreSQL, RabbitMQ i Redis.
3. Dodać ServiceDefaults i podpiąć go do wszystkich serwisów .NET.
4. Utworzyć projekty API z minimalnymi endpointami health/readiness.
5. Utworzyć aplikacje React z TypeScript, Vite i wspólną konfiguracją lint/test.
6. Podpiąć aplikacje Node do AppHost jako uruchamiane zasoby.
7. Dodać krótki README developerski z komendą uruchomienia środowiska.

## Kryteria akceptacji

- AppHost uruchamia wszystkie zasoby bez ręcznego startowania brokerów i baz.
- Każdy serwis ma health check widoczny w Aspire Dashboard.
- Każda aplikacja frontendowa startuje lokalnie z AppHost.
- Konfiguracja połączeń jest przekazywana przez Aspire, bez hardcodowanych connection stringów.
- Repozytorium ma jedną opisaną ścieżkę uruchomienia dla dewelopera.

## Scenariusze testowe

- Start AppHost z czystego środowiska developerskiego.
- Sprawdzenie health endpointów wszystkich API.
- Sprawdzenie dostępności dashboardu Aspire i logów aplikacji.
- Zatrzymanie RabbitMQ i potwierdzenie, że zależne serwisy pokazują niezdrowy stan.
