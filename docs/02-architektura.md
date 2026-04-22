# Architektura Mikroserwisów: Aplikacja do zarządzania Dark Kitchen

```mermaid
graph TD
graph TD
    %% Warstwa Zewnętrzna
    subgraph External_Entry [Zewnętrzne Źródła Zamówień]
        Glovo[<b>Glovo</b><br/>Webhook API]
        UberEats[<b>Uber Eats</b><br/>Webhook API]
        Pyszne[<b>Pyszne.pl</b><br/>Webhook API]
    end

    %% Domena Sprzedaży
    subgraph Domain_Storefront [Domena Sprzedaży Bezpośredniej]
        StoreApp[<b>Sklep Klienta</b><br/>Web/Mobile App]
        StorefrontSvc[Storefront Service]
        StorefrontDB[(Storefront DB)]

        StoreApp -->|HTTPS| StorefrontSvc
        StorefrontSvc --- StorefrontDB
    end

    %% Domena Katalogu
    subgraph Domain_Catalog [Domena Katalogu i Konfiguracji]
        AdminApp[<b>Panel Managera</b><br/>Web Admin]
        CatalogSvc[Catalog & Recipe Service]
        CatalogDB[(Catalog DB)]

        AdminApp -->|HTTPS| CatalogSvc
        CatalogSvc --- CatalogDB
    end

    %% Domena Zamówień
    subgraph Domain_Order [Centrum Zarządzania Zamówieniami]
        OrderSvc[Order Management Service]
        OrderDB[(Order DB)]

        OrderSvc --- OrderDB
    end

    %% Domena Magazynu
    subgraph Domain_Inventory [Domena Magazynu]
        InvSvc[Inventory Service]
        InvDB[(Inv DB)]

        InvSvc --- InvDB
    end

    %% Domena Kuchni
    subgraph Domain_KDS [Domena Kuchni]
        KitchenApp[<b>Aplikacja Stacji</b><br/>Tablety Kucharzy]
        KDSSvc[KDS Service]
        KDSDB[(KDS DB)]

        KitchenApp <-->|WebSockets| KDSSvc
        KDSSvc --- KDSDB
    end

    %% Domena Wydawki
    subgraph Domain_Packing [Domena Wydawki]
        PackingApp[<b>Aplikacja Wydawki</b><br/>Terminal Pakowania]
        PackingSvc[Packing Service]
        PackingDB[(Packing DB)]

        PackingApp <-->|WebSockets| PackingSvc
        PackingSvc --- PackingDB
    end

    %% Warstwa Infrastruktury
    Broker[[<b>Message Broker</b><br/>RabbitMQ / Kafka]]

    %% PRZEPŁYWY MIĘDZYDOMENOWE

    %% 1. Wpadanie zamówień z zewnątrz
    Glovo -->|JSON Webhook| OrderSvc
    UberEats -->|JSON Webhook| OrderSvc
    Pyszne -->|JSON Webhook| OrderSvc

    %% 2. Wpadanie zamówień ze sklepu własnego
    StorefrontSvc -->|Internal API / Event| OrderSvc

    %% 3. Szyna Zdarzeń (Event Bus) - Komunikacja asynchroniczna
    OrderSvc -.->|OrderPlaced / OrderAccepted| Broker
    CatalogSvc -.->|RecipeCreated / PriceChanged| Broker
    InvSvc -.->|InventoryReserved / OutOfStock| Broker
    KDSSvc -.->|ItemReady| Broker
    PackingSvc -.->|OrderReadyForPickup| Broker

    Broker -.-> OrderSvc
    Broker -.-> InvSvc
    Broker -.-> KDSSvc
    Broker -.-> PackingSvc
    Broker -.-> StorefrontSvc

    %% 4. Integracja pomocnicza
    StorefrontSvc -.->|Pobieranie Menu| CatalogSvc
```
