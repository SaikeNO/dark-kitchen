# Use case'y serwisow

Dokument zbiera use case'y wykryte w obecnym stanie repozytorium. Zrodla: endpointy Minimal API, handlery zdarzen integracyjnych, aplikacje webowe i testy integracyjne.

## Legenda

| Pole | Znaczenie |
| --- | --- |
| ID | Stabilny identyfikator use case'a w dokumentacji. |
| Trigger | Aktor, endpoint, UI albo zdarzenie uruchamiajace przypadek. |
| Typ | `command`, `query`, `event-driven`, `ui-workflow`, `technical`. |
| Stan | `zaimplementowany`, `czesciowy`, `planowany`. |
| Dowod | Plik albo obszar repo potwierdzajacy implementacje. |

## Mapa serwisow

| Serwis | Frontend | Rola |
| --- | --- | --- |
| Catalog & Recipe Service | `src/Web/admin-panel` | Master data: marki, menu, produkty, receptury, skladniki, stacje, routing kuchenny, assety. |
| Storefront Service | `src/Web/storefront` | Publiczny sklep klienta: menu, koszyk, klient, checkout. |
| Order Management Service | brak dedykowanego UI | Przyjecie zamowien, normalizacja, stan zamowienia, orkiestracja po zdarzeniach. |
| Inventory Service | `src/Web/inventory-panel` | Stan magazynu, progi, dostawy, korekty, rezerwacje pod zamowienia. |
| KDS Service | `src/Web/kitchen-app` | Tablica kuchni: stacje, zadania, start/zakonczenie przygotowania. |
| Packing Service | `src/Web/packing-terminal` | Terminal wydawki: manifesty, kompletowanie i wydanie zamowienia. |

## Przeplywy miedzyserwisowe

| ID | Przeplyw | Serwisy | Zdarzenia |
| --- | --- | --- | --- |
| FLOW-001 | Publikacja katalogu do read modeli | Catalog -> Storefront, OrderManagement | `brand.changed`, `category.changed`, `menu.item_changed`, `product.price_changed` |
| FLOW-002 | Publikacja receptury do magazynu | Catalog -> Inventory | `recipe.changed` |
| FLOW-003 | Publikacja stacji i routingu do KDS | Catalog -> KDS | `station.changed`, `product.station_routing_changed` |
| FLOW-004 | Zamowienie ze sklepu | Storefront -> OrderManagement -> Inventory -> OrderManagement -> KDS/Packing | `order.placed`, `inventory.reserved`, `order.accepted` |
| FLOW-005 | Zamowienie z mock delivery | OrderManagement -> Inventory -> OrderManagement -> KDS/Packing | `order.placed`, `inventory.reserved`, `order.accepted` |
| FLOW-006 | Postep przygotowania | KDS -> OrderManagement/Packing | `item.preparation_started`, `item.preparation_completed`, `order.ready_for_packing` |
| FLOW-007 | Wydanie zamowienia | Packing -> OrderManagement | `order.ready_for_pickup` |

## Catalog & Recipe Service

### Rola

Serwis jest zrodlem prawdy dla katalogu, receptur i konfiguracji kuchni. Udostepnia panel administracyjny, publiczne menu i zdarzenia dla read modeli innych serwisow.

### Wejscia i wyjscia

| Kierunek | Elementy |
| --- | --- |
| HTTP | `/api/admin/auth/*`, `/api/admin/brands`, `/api/admin/categories`, `/api/admin/products`, `/api/admin/ingredients`, `/api/admin/products/{productId}/recipe`, `/api/admin/stations`, `/api/admin/products/{productId}/station-route`, `/api/admin/uploads/{kind}`, `/api/menu/brands/{brandId}` |
| UI | `src/Web/admin-panel` |
| Publikuje | `brand.changed`, `category.changed`, `menu.item_changed`, `product.price_changed`, `recipe.changed`, `station.changed`, `product.station_routing_changed` |
| Konsumuje | Brak zdarzen w topologii. |

### Use case'y

| ID | Nazwa | Trigger | Typ | Stan | Dowod |
| --- | --- | --- | --- | --- | --- |
| UC-CAT-001 | Logowanie administratora | `POST /api/admin/auth/login` | command | zaimplementowany | `Features/Auth/LoginAdminEndpoint.cs`, `admin-panel/features/auth` |
| UC-CAT-002 | Wylogowanie administratora | `POST /api/admin/auth/logout` | command | zaimplementowany | `Features/Auth/LogoutAdminEndpoint.cs` |
| UC-CAT-003 | Pobranie sesji administratora | `GET /api/admin/auth/me` | query | zaimplementowany | `Features/Auth/GetCurrentAdminUserEndpoint.cs` |
| UC-CAT-004 | Lista marek | `GET /api/admin/brands` | query | zaimplementowany | `Features/Brands/ListBrandsEndpoint.cs` |
| UC-CAT-005 | Utworzenie marki | `POST /api/admin/brands` | command | zaimplementowany | `Features/Brands/CreateBrandEndpoint.cs` |
| UC-CAT-006 | Edycja marki | `PUT /api/admin/brands/{brandId}` | command | zaimplementowany | `Features/Brands/UpdateBrandEndpoint.cs` |
| UC-CAT-007 | Dezaktywacja marki | `POST /api/admin/brands/{brandId}/deactivate` | command | zaimplementowany | `Features/Brands/DeactivateBrandEndpoint.cs` |
| UC-CAT-008 | Lista kategorii menu | `GET /api/admin/categories` | query | zaimplementowany | `Features/Categories/ListCategoriesEndpoint.cs` |
| UC-CAT-009 | Utworzenie kategorii | `POST /api/admin/categories` | command | zaimplementowany | `Features/Categories/CreateCategoryEndpoint.cs` |
| UC-CAT-010 | Edycja kategorii | `PUT /api/admin/categories/{categoryId}` | command | zaimplementowany | `Features/Categories/UpdateCategoryEndpoint.cs` |
| UC-CAT-011 | Dezaktywacja kategorii | `POST /api/admin/categories/{categoryId}/deactivate` | command | zaimplementowany | `Features/Categories/DeactivateCategoryEndpoint.cs` |
| UC-CAT-012 | Lista produktow | `GET /api/admin/products` | query | zaimplementowany | `Features/Products/ListProductsEndpoint.cs` |
| UC-CAT-013 | Utworzenie produktu | `POST /api/admin/products` | command | zaimplementowany | `Features/Products/CreateProductEndpoint.cs` |
| UC-CAT-014 | Edycja produktu | `PUT /api/admin/products/{productId}` | command | zaimplementowany | `Features/Products/UpdateProductEndpoint.cs` |
| UC-CAT-015 | Aktywacja produktu | `POST /api/admin/products/{productId}/activate` | command | zaimplementowany | `Features/Products/ActivateProductEndpoint.cs` |
| UC-CAT-016 | Dezaktywacja produktu | `POST /api/admin/products/{productId}/deactivate` | command | zaimplementowany | `Features/Products/DeactivateProductEndpoint.cs` |
| UC-CAT-017 | Lista skladnikow | `GET /api/admin/ingredients` | query | zaimplementowany | `Features/Ingredients/ListIngredientsEndpoint.cs` |
| UC-CAT-018 | Utworzenie skladnika | `POST /api/admin/ingredients` | command | zaimplementowany | `Features/Ingredients/CreateIngredientEndpoint.cs` |
| UC-CAT-019 | Edycja skladnika | `PUT /api/admin/ingredients/{ingredientId}` | command | zaimplementowany | `Features/Ingredients/UpdateIngredientEndpoint.cs` |
| UC-CAT-020 | Dezaktywacja skladnika | `POST /api/admin/ingredients/{ingredientId}/deactivate` | command | zaimplementowany | `Features/Ingredients/DeactivateIngredientEndpoint.cs` |
| UC-CAT-021 | Pobranie receptury produktu | `GET /api/admin/products/{productId}/recipe` | query | zaimplementowany | `Features/Recipes/GetRecipeEndpoint.cs` |
| UC-CAT-022 | Zapis receptury produktu | `PUT /api/admin/products/{productId}/recipe` | command | zaimplementowany | `Features/Recipes/UpsertRecipeEndpoint.cs` |
| UC-CAT-023 | Lista stacji kuchennych | `GET /api/admin/stations` | query | zaimplementowany | `Features/Stations/ListStationsEndpoint.cs` |
| UC-CAT-024 | Utworzenie stacji kuchennej | `POST /api/admin/stations` | command | zaimplementowany | `Features/Stations/CreateStationEndpoint.cs` |
| UC-CAT-025 | Edycja stacji kuchennej | `PUT /api/admin/stations/{stationId}` | command | zaimplementowany | `Features/Stations/UpdateStationEndpoint.cs` |
| UC-CAT-026 | Dezaktywacja stacji kuchennej | `POST /api/admin/stations/{stationId}/deactivate` | command | zaimplementowany | `Features/Stations/DeactivateStationEndpoint.cs` |
| UC-CAT-027 | Ustawienie routingu produktu na stacje | `PUT /api/admin/products/{productId}/station-route` | command | zaimplementowany | `Features/ProductStationRoutes/UpsertProductStationRouteEndpoint.cs` |
| UC-CAT-028 | Upload assetu | `POST /api/admin/uploads/{kind}` | command | zaimplementowany | `Features/Uploads/UploadAssetEndpoint.cs` |
| UC-CAT-029 | Pobranie assetu | `GET /uploads/{kind}/{fileName}` | query | zaimplementowany | `Features/Uploads/GetUploadedAssetEndpoint.cs` |
| UC-CAT-030 | Publiczne menu marki | `GET /api/menu/brands/{brandId}` | query | zaimplementowany | `Features/PublicMenu/GetBrandMenuEndpoint.cs` |
| UC-CAT-031 | Panel managera: zarzadzanie katalogiem | `admin-panel` | ui-workflow | zaimplementowany | `src/Web/admin-panel/src/App.tsx`, `features/*` |
| UC-CAT-032 | Info serwisu | `GET /`, `GET /api/info` | technical | zaimplementowany | `Features/ServiceInfo/ServiceInfoEndpoints.cs` |

### Zdarzenia i efekty

| Zdarzenie | Powod |
| --- | --- |
| `brand.changed` | Utworzenie, edycja albo dezaktywacja marki. |
| `category.changed` | Utworzenie, edycja albo dezaktywacja kategorii. |
| `menu.item_changed` | Utworzenie, edycja, aktywacja albo dezaktywacja produktu. |
| `product.price_changed` | Zmiana ceny produktu. |
| `recipe.changed` | Zapis receptury produktu. |
| `station.changed` | Utworzenie, edycja albo dezaktywacja stacji. |
| `product.station_routing_changed` | Zmiana stacji dla produktu. |

### Luki

| Obszar | Luka |
| --- | --- |
| Audyt | Brak osobnego use case'a historii zmian katalogu. |
| Aktywacja marek/kategorii/skladnikow/stacji | Wystepuje dezaktywacja; osobna aktywacja jest widoczna tylko dla produktu. |
| Uprawnienia | Sa role `Operator` i `Manager`; dokumentacja powinna pozniej doprecyzowac macierz praw. |

## Storefront Service

### Rola

Serwis obsluguje publiczny sklep klienta. Ma lokalne read modele katalogu, koszyk, konto klienta i checkout do Order Management.

### Wejscia i wyjscia

| Kierunek | Elementy |
| --- | --- |
| HTTP | `/api/storefront/brands`, `/api/storefront/context`, `/api/storefront/menu`, `/api/storefront/auth/*`, `/api/storefront/carts`, `/api/storefront/checkout`, `/api/storefront/orders`, `/api/storefront/orders/{orderId}` |
| UI | `src/Web/storefront` |
| Konsumuje | `brand.changed`, `category.changed`, `menu.item_changed`, `product.price_changed`, `inventory.reservation_failed`, `order.accepted`, `item.preparation_started`, `order.ready_for_packing`, `order.ready_for_pickup`, `order.completed` |
| Zaleznosci | Order Management przez `OrderManagement__BaseUrl` dla checkoutu. |

### Use case'y

| ID | Nazwa | Trigger | Typ | Stan | Dowod |
| --- | --- | --- | --- | --- | --- |
| UC-SF-001 | Lista marek dla klienta | `GET /api/storefront/brands` | query | zaimplementowany | `Features/Menu/ListStorefrontBrandsEndpoint.cs` |
| UC-SF-002 | Kontekst marki | `GET /api/storefront/context?brandId=...` | query | zaimplementowany | `Features/Menu/GetStorefrontContextEndpoint.cs` |
| UC-SF-003 | Menu marki | `GET /api/storefront/menu?brandId=...` | query | zaimplementowany | `Features/Menu/GetStorefrontMenuEndpoint.cs` |
| UC-SF-004 | Rejestracja klienta | `POST /api/storefront/auth/register` | command | zaimplementowany | `Features/Auth/RegisterCustomerEndpoint.cs` |
| UC-SF-005 | Logowanie klienta | `POST /api/storefront/auth/login` | command | zaimplementowany | `Features/Auth/LoginCustomerEndpoint.cs` |
| UC-SF-006 | Pobranie sesji klienta | `GET /api/storefront/auth/me` | query | zaimplementowany | `Features/Auth/GetCurrentCustomerEndpoint.cs` |
| UC-SF-007 | Wylogowanie klienta | `POST /api/storefront/auth/logout` | command | zaimplementowany | `Features/Auth/LogoutCustomerEndpoint.cs` |
| UC-SF-008 | Utworzenie albo odtworzenie koszyka | `POST /api/storefront/carts?brandId=...` | command | zaimplementowany | `Features/Carts/CreateCartEndpoint.cs` |
| UC-SF-009 | Pobranie koszyka | `GET /api/storefront/carts/{cartId}?brandId=...` | query | zaimplementowany | `Features/Carts/GetCartEndpoint.cs` |
| UC-SF-010 | Aktualizacja pozycji koszyka | `PATCH /api/storefront/carts/{cartId}?brandId=...` | command | zaimplementowany | `Features/Carts/UpdateCartEndpoint.cs` |
| UC-SF-011 | Checkout z mock platnoscia | `POST /api/storefront/checkout?brandId=...` | command | zaimplementowany | `Features/Checkout/CheckoutEndpoint.cs` |
| UC-SF-012 | Synchronizacja katalogu marki | `brand.changed` | event-driven | zaimplementowany | `Features/Catalog/BrandChangedHandler.cs` |
| UC-SF-013 | Synchronizacja kategorii | `category.changed` | event-driven | zaimplementowany | `Features/Catalog/CategoryChangedHandler.cs` |
| UC-SF-014 | Synchronizacja produktu i ceny | `menu.item_changed`, `product.price_changed` | event-driven | zaimplementowany | `Features/Catalog/MenuItemChangedHandler.cs`, `ProductPriceChangedHandler.cs` |
| UC-SF-015 | UI sklepu: wybor marki, koszyk, checkout, konto | `storefront` | ui-workflow | zaimplementowany | `src/Web/storefront/src/App.tsx` |
| UC-SF-016 | Info serwisu | `GET /`, `GET /api/info` | technical | zaimplementowany | `Features/ServiceInfo/ServiceInfoEndpoints.cs` |
| UC-SF-017 | Status i historia zamowien klienta | `GET /api/storefront/orders`, lifecycle eventy | query/event-driven/ui-workflow | zaimplementowany | `Features/Orders/*`, `src/Web/storefront/src/App.tsx` |

### Luki

| Obszar | Luka |
| --- | --- |
| Platnosci | Checkout uzywa mock wyniku platnosci; realny provider jest poza obecnym zakresem. |

## Order Management Service

### Rola

Serwis przyjmuje zamowienia ze sklepu i integracji delivery, zapisuje je, publikuje `order.placed` i prowadzi stan globalny po odpowiedziach z magazynu, kuchni i wydawki.

### Wejscia i wyjscia

| Kierunek | Elementy |
| --- | --- |
| HTTP | `/api/orders/storefront`, `/api/mock-delivery/webhooks/orders`, `/api/orders/{orderId}`, `/api/orders/{orderId}/cancel` |
| Konsumuje | `inventory.reserved`, `inventory.reservation_failed`, `menu.item_changed`, `product.price_changed`, `item.preparation_started`, `order.ready_for_packing`, `order.ready_for_pickup`, `order.completed` |
| Publikuje | `order.placed`, `order.accepted`, `order.cancelled` |

### Use case'y

| ID | Nazwa | Trigger | Typ | Stan | Dowod |
| --- | --- | --- | --- | --- | --- |
| UC-OMS-001 | Utworzenie zamowienia ze Storefront | `POST /api/orders/storefront` | command | zaimplementowany | `Features/Orders/CreateStorefrontOrderEndpoint.cs` |
| UC-OMS-002 | Utworzenie zamowienia z mock delivery | `POST /api/mock-delivery/webhooks/orders` | command | zaimplementowany | `Features/Orders/CreateMockDeliveryOrderEndpoint.cs` |
| UC-OMS-003 | Pobranie zamowienia | `GET /api/orders/{orderId}` | query | zaimplementowany | `Features/Orders/GetOrderEndpoint.cs` |
| UC-OMS-004 | Wspolna normalizacja i zapis zamowienia | endpointy zamowien | command | zaimplementowany | `Features/Orders/CreateOrderHandler.cs` |
| UC-OMS-005 | Publikacja zamowienia do procesu rezerwacji | zapis zamowienia | event-driven | zaimplementowany | `Application/OrderManagementEventFactory.cs` |
| UC-OMS-006 | Akceptacja po rezerwacji magazynu | `inventory.reserved` | event-driven | zaimplementowany | `Features/Inventory/InventoryReservedHandler.cs` |
| UC-OMS-007 | Odrzucenie po braku rezerwacji | `inventory.reservation_failed` | event-driven | zaimplementowany | `Features/Inventory/InventoryReservationFailedHandler.cs` |
| UC-OMS-008 | Aktualizacja snapshotu produktu | `menu.item_changed` | event-driven | zaimplementowany | `Features/Menu/MenuItemChangedHandler.cs` |
| UC-OMS-009 | Aktualizacja snapshotu ceny | `product.price_changed` | event-driven | zaimplementowany | `Features/Menu/ProductPriceChangedHandler.cs` |
| UC-OMS-010 | Oznaczenie startu przygotowania | `item.preparation_started` | event-driven | zaimplementowany | `Features/Progress/ItemPreparationStartedHandler.cs` |
| UC-OMS-011 | Oznaczenie gotowosci do pakowania | `order.ready_for_packing` | event-driven | zaimplementowany | `Features/Progress/OrderReadyForPackingHandler.cs` |
| UC-OMS-012 | Oznaczenie gotowosci do odbioru | `order.ready_for_pickup` | event-driven | zaimplementowany | `Features/Progress/OrderReadyForPickupHandler.cs` |
| UC-OMS-013 | Info serwisu | `GET /`, `GET /api/info` | technical | zaimplementowany | `Features/ServiceInfo/ServiceInfoEndpoints.cs` |
| UC-OMS-014 | Anulowanie zamowienia | `POST /api/orders/{orderId}/cancel` | command/event-driven | zaimplementowany | `Features/Orders/CancelOrderEndpoint.cs` |
| UC-OMS-015 | Finalizacja zamowienia | `order.completed` | event-driven | zaimplementowany | `Features/Progress/OrderCompletedHandler.cs` |

### Stany zamowienia

| Stan | Wejscie |
| --- | --- |
| `Placed` | Zamowienie zapisane i opublikowane jako `order.placed`. |
| `Accepted` | `inventory.reserved`. |
| `Rejected` | `inventory.reservation_failed`. |
| `Preparing` | `item.preparation_started`. |
| `ReadyForPacking` | `order.ready_for_packing`. |
| `ReadyForPickup` | `order.ready_for_pickup`. |
| `Completed` | `order.completed`. |

### Luki

| Obszar | Luka |
| --- | --- |
| Real delivery adapters | Jest mock delivery; realne adaptery sa planowane w dokumentacji integracji. |

## Inventory Service

### Rola

Serwis przechowuje stan magazynu, snapshoty receptur i rezerwuje skladniki pod zamowienia.

### Wejscia i wyjscia

| Kierunek | Elementy |
| --- | --- |
| HTTP | `/api/admin/inventory/items`, `/api/admin/inventory/shortages`, `/api/admin/inventory/items/{ingredientId}/delivery`, `/api/admin/inventory/items/{ingredientId}/adjustment` |
| UI | `src/Web/inventory-panel` |
| Konsumuje | `order.placed`, `recipe.changed`, `order.cancelled`, `order.completed` |
| Publikuje | `inventory.reserved`, `inventory.reservation_failed` |

### Use case'y

| ID | Nazwa | Trigger | Typ | Stan | Dowod |
| --- | --- | --- | --- | --- | --- |
| UC-INV-001 | Lista pozycji magazynowych | `GET /api/admin/inventory/items` | query | zaimplementowany | `Features/InventoryAdmin/ListInventoryItemsEndpoint.cs` |
| UC-INV-002 | Lista brakow magazynowych | `GET /api/admin/inventory/shortages` | query | zaimplementowany | `Features/InventoryAdmin/ListShortagesEndpoint.cs` |
| UC-INV-003 | Rejestracja dostawy | `POST /api/admin/inventory/items/{ingredientId}/delivery` | command | zaimplementowany | `Features/InventoryAdmin/RecordDeliveryEndpoint.cs` |
| UC-INV-004 | Korekta stanu i progu bezpieczenstwa | `POST /api/admin/inventory/items/{ingredientId}/adjustment` | command | zaimplementowany | `Features/InventoryAdmin/AdjustInventoryItemEndpoint.cs` |
| UC-INV-005 | Synchronizacja receptury i utworzenie pozycji magazynu | `recipe.changed` | event-driven | zaimplementowany | `Features/Recipes/RecipeChangedHandler.cs` |
| UC-INV-006 | Rezerwacja skladnikow pod zamowienie | `order.placed` | event-driven | zaimplementowany | `Features/Orders/OrderPlacedHandler.cs` |
| UC-INV-007 | Publikacja sukcesu rezerwacji | wynik UC-INV-006 | event-driven | zaimplementowany | `Application/InventoryEventFactory.cs` |
| UC-INV-008 | Publikacja bledu rezerwacji | brak receptury albo skladnika | event-driven | zaimplementowany | `Application/InventoryReasonCodes.cs`, `OrderPlacedHandler.cs` |
| UC-INV-009 | Panel magazynu: filtrowanie, dostawy, korekty | `inventory-panel` | ui-workflow | zaimplementowany | `src/Web/inventory-panel/src/App.tsx` |
| UC-INV-010 | Info serwisu | `GET /`, `GET /api/info` | technical | zaimplementowany | `Features/ServiceInfo/ServiceInfoEndpoints.cs` |
| UC-INV-011 | Zwolnienie rezerwacji po anulowaniu | `order.cancelled` | event-driven | zaimplementowany | `Features/Orders/ReservationLifecycleHandlers.cs` |
| UC-INV-012 | Rozchod zarezerwowanego stanu po finalizacji | `order.completed` | event-driven | zaimplementowany | `Features/Orders/ReservationLifecycleHandlers.cs` |

### Luki

| Obszar | Luka |
| --- | --- |
| Autoryzacja admin inventory | Endpointy chronione operacyjna polityka `ops.operator`; docelowo mozna podpiac wspolne Identity pracownicze. |

## KDS Service

### Rola

Serwis buduje zadania kuchenne z zaakceptowanych zamowien, wyswietla je na stacjach i publikuje postep przygotowania.

### Wejscia i wyjscia

| Kierunek | Elementy |
| --- | --- |
| HTTP | `/api/kitchen/stations`, `/api/kitchen/stations/{stationId}/tasks`, `/api/kitchen/tasks/{taskId}/start`, `/api/kitchen/tasks/{taskId}/done` |
| UI | `src/Web/kitchen-app` |
| SignalR | `/hubs/kitchen` |
| Konsumuje | `order.accepted`, `station.changed`, `product.station_routing_changed` |
| Publikuje | `item.preparation_started`, `item.preparation_completed` |

### Use case'y

| ID | Nazwa | Trigger | Typ | Stan | Dowod |
| --- | --- | --- | --- | --- | --- |
| UC-KDS-001 | Synchronizacja stacji kuchennej | `station.changed` | event-driven | zaimplementowany | `Features/Catalog/StationChangedHandler.cs` |
| UC-KDS-002 | Synchronizacja routingu produktu | `product.station_routing_changed` | event-driven | zaimplementowany | `Features/Catalog/ProductStationRoutingChangedHandler.cs` |
| UC-KDS-003 | Utworzenie ticketu i zadan kuchennych | `order.accepted` | event-driven | zaimplementowany | `Features/Orders/OrderAcceptedHandler.cs` |
| UC-KDS-004 | Lista aktywnych stacji | `GET /api/kitchen/stations` | query | zaimplementowany | `Features/Kitchen/KitchenEndpoints.cs` |
| UC-KDS-005 | Lista zadan stacji | `GET /api/kitchen/stations/{stationId}/tasks` | query | zaimplementowany | `Features/Kitchen/KitchenEndpoints.cs` |
| UC-KDS-006 | Start zadania kuchennego | `POST /api/kitchen/tasks/{taskId}/start` | command | zaimplementowany | `Features/Kitchen/KitchenTaskActions.cs` |
| UC-KDS-007 | Zakonczenie zadania kuchennego | `POST /api/kitchen/tasks/{taskId}/done` | command | zaimplementowany | `Features/Kitchen/KitchenTaskActions.cs` |
| UC-KDS-008 | Powiadomienie UI o zmianie zadania | SignalR | event-driven | zaimplementowany | `Features/Kitchen/KitchenTaskNotifier.cs`, `KitchenHub.cs` |
| UC-KDS-009 | Aplikacja kuchni: wybor stacji i tablica zadan | `kitchen-app` | ui-workflow | zaimplementowany | `src/Web/kitchen-app/src/App.tsx` |
| UC-KDS-010 | Info serwisu | `GET /`, `GET /api/info` | technical | zaimplementowany | `Features/ServiceInfo/ServiceInfoEndpoints.cs` |

### Luki

| Obszar | Luka |
| --- | --- |
| Brak routingu | Zadania moga powstac jako routing missing; brakuje UI/operacji naprawczej po stronie KDS. |
| Przypisanie kucharza | Brak identyfikacji pracownika startujacego/konczacego zadanie. |
| Autoryzacja kuchni | Endpointy chronione operacyjna polityka `ops.operator`; docelowo mozna podpiac wspolne Identity pracownicze. |

## Packing Service

### Rola

Serwis tworzy manifest pakowania, sledzi kompletowanie pozycji po zdarzeniach z KDS i publikuje gotowosc do odbioru po wydaniu.

### Wejscia i wyjscia

| Kierunek | Elementy |
| --- | --- |
| HTTP | `/api/packing/manifests`, `/api/packing/manifests/{manifestId}/issued` |
| UI | `src/Web/packing-terminal` |
| SignalR | `/hubs/packing` |
| Konsumuje | `order.accepted`, `item.preparation_completed` |
| Publikuje | `order.ready_for_packing`, `order.ready_for_pickup` |

### Use case'y

| ID | Nazwa | Trigger | Typ | Stan | Dowod |
| --- | --- | --- | --- | --- | --- |
| UC-PACK-001 | Utworzenie manifestu pakowania | `order.accepted` | event-driven | zaimplementowany | `Features/Packing/OrderAcceptedHandler.cs` |
| UC-PACK-002 | Oznaczenie pozycji jako gotowej | `item.preparation_completed` | event-driven | zaimplementowany | `Features/Packing/ItemPreparationCompletedHandler.cs` |
| UC-PACK-003 | Publikacja gotowosci do pakowania | wszystkie pozycje gotowe | event-driven | zaimplementowany | `Features/Packing/PackingManifestActions.cs` |
| UC-PACK-004 | Lista aktywnych manifestow | `GET /api/packing/manifests` | query | zaimplementowany | `Features/Packing/PackingEndpoints.cs` |
| UC-PACK-005 | Wydanie manifestu z walidacja kodu odbioru | `POST /api/packing/manifests/{manifestId}/issued` | command | zaimplementowany | `Features/Packing/PackingEndpoints.cs` |
| UC-PACK-006 | Publikacja gotowosci do odbioru i finalizacji | wydanie manifestu | event-driven | zaimplementowany | `Application/PackingEventFactory.cs` |
| UC-PACK-007 | Powiadomienie terminala o zmianie manifestu | SignalR | event-driven | zaimplementowany | `Features/Packing/PackingManifestNotifier.cs`, `PackingHub.cs` |
| UC-PACK-008 | Terminal wydawki: kolumny manifestow i wydanie | `packing-terminal` | ui-workflow | zaimplementowany | `src/Web/packing-terminal/src/App.tsx` |
| UC-PACK-009 | Info serwisu | `GET /`, `GET /api/info` | technical | zaimplementowany | `Features/ServiceInfo/ServiceInfoEndpoints.cs` |

### Luki

| Obszar | Luka |
| --- | --- |
| Czesc wydania | Brak osobnego use case'a czesciowego wydania albo cofniecia wydania. |
| Autoryzacja wydawki | Endpointy chronione operacyjna polityka `ops.operator`; docelowo mozna podpiac wspolne Identity pracownicze. |

## Macierz pokrycia

| Obszar | Endpointy | Handlery eventow | UI | Testy integracyjne |
| --- | --- | --- | --- | --- |
| Catalog | tak | publikuje zdarzenia | `admin-panel` | `tests/Integration/Catalog` |
| Storefront | tak | katalogowe read modele | `storefront` | `tests/Integration/Storefront` |
| OrderManagement | tak | lifecycle zamowienia | brak | `tests/Integration/OrderManagement` |
| Inventory | tak | receptury i rezerwacje | `inventory-panel` | `tests/Integration/Inventory` |
| KDS | tak | ticket, stacje, routing | `kitchen-app` | `tests/Integration/Kds` |
| Packing | tak | manifest i wydanie | `packing-terminal` | `tests/Integration/Packing` |
| End-to-end | czesciowo przez AppHost | tak | czesciowo | `tests/Integration/AppHost/EndToEndOrderFlowTests.cs` |

## Backlog z luk

| ID | Serwis | Brak | Ryzyko | Proponowana akcja |
| --- | --- | --- | --- | --- |
| GAP-001 | Storefront | Real-time status zamowienia | Klient widzi polling statusu, ale brak SignalR/push. | Dodac push statusu klienta, jesli UX bedzie tego wymagal. |
| GAP-002 | Inventory | Rezerwacje po odrzuceniu pozniejszego etapu | Obecnie obslugiwane anulowanie i completed; brak osobnego eventu reject po akceptacji. | Dodac zdarzenie kompensacji, gdy pojawi sie odrzucenie po rezerwacji. |
| GAP-003 | OrderManagement | Produkcyjne potwierdzenie odbioru | `Completed` przychodzi z terminala packing; brak realnego adaptera kuriera. | Podpiac realny proces odbioru w integracjach delivery. |
| GAP-004 | KDS | Naprawa brakujacego routingu | Zamowienie moze nie trafic na stacje. | Dodac widok/operacje routing missing albo blokade publikacji menu bez routingu. |
| GAP-005 | Packing | Czesc wydania/cofniecie wydania | Brak procedury korekty po pomylce na wydawce. | Dodac command cofniecia albo reklamacji operacyjnej. |
| GAP-006 | Admin/KDS/Packing/Inventory | Produkcyjne Identity operacyjne | Jest wspolna polityka naglowkowa dla paneli operacyjnych. | Zastapic ja docelowym SSO/Identity dla pracownikow. |
