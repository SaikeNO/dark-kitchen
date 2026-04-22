# ADR 002: Wybór .NET Aspire do orkiestracji i zarządzania lokalnym środowiskiem deweloperskim

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Zgodnie z decyzją podjętą w ADR 001, nasz system zarządzania Dark Kitchen będzie oparty na architekturze mikroserwisów oraz komunikacji asynchronicznej (Event-Driven). Wraz z rozwojem projektu, środowisko lokalne będzie składać się z wielu współpracujących ze sobą elementów: minimum 4-5 mikroserwisów, brokera wiadomości (np. RabbitMQ), relacyjnych baz danych (np. PostgreSQL), magazynów klucz-wartość (np. Redis dla KDS) oraz aplikacji frontendowych/mobilnych.

Zarządzanie takim stosem lokalnie – uruchamianie wielu procesów, ręczne konfigurowanie _connection stringów_, dbanie o odpowiednią kolejność startu usług oraz (co najważniejsze w systemach rozproszonych) śledzenie logów i przepływu zdarzeń między serwisami – jest zadaniem trudnym i spowalniającym rozwój (tzw. "developer friction"). Tradycyjne podejście oparte wyłącznie na plikach `docker-compose` bywa kłopotliwe w debugowaniu kodu aplikacji w czasie rzeczywistym.

## 3. Decyzja

Decydujemy się na wykorzystanie platformy **.NET Aspire** jako głównego narzędzia do orkiestracji środowiska deweloperskiego, zarządzania zależnościami (usługami wspierającymi) oraz zapewnienia wbudowanej obserwowalności (observability) całego systemu lokalnie.

## 4. Uzasadnienie

- **Uproszczona konfiguracja (AppHost):** .NET Aspire pozwala zdefiniować całą infrastrukturę aplikacji w kodzie C# (projekt `AppHost`). Dzięki temu w jednym pliku deklarujemy, że potrzebujemy RabbitMQ, instancji PostgreSQL i naszych mikroserwisów, a Aspire automatycznie zarządza ich uruchomieniem i wstrzykuje odpowiednie zmienne środowiskowe do komunikacji (Service Discovery).
- **Wbudowana Obserwowalność (Observability):** Rozwiązywanie problemów w komunikacji opartej na zdarzeniach (np. gdy `OrderCreated` zostało wysłane, ale KDS Service na nie nie zareagował) wymaga doskonałego śledzenia żądań (Distributed Tracing). Aspire dostarcza gotowy, lokalny Dashboard integrujący logi, metryki i ślady (OpenTelemetry) "wyjęte z pudełka", bez konieczności stawiania na start skomplikowanego stosu ELK czy Jaeger.
- **Standaryzacja (Service Defaults):** Konfiguracja wspólna dla wszystkich serwisów (np. endpointy dla telemetrii, health checki, odporność na błędy dzięki bibliotece Polly) znajduje się w jednym współdzielonym projekcie `ServiceDefaults`.
- **Gotowe integracje (Hosting Components):** Aspire posiada przygotowane pakiety NuGet dla popularnych usług (np. `Aspire.RabbitMQ.Client`, `Aspire.Hosting.PostgreSQL`), które automatycznie konfigurują optymalne ustawienia i logowanie.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Drastyczne przyspieszenie wdrażania nowych programistów do projektu (uruchomienie całości sprowadza się do kliknięcia F5 w środowisku IDE).
- Genialne doświadczenie deweloperskie (DX) przy debugowaniu systemów rozproszonych dzięki Aspire Dashboard.
- Gotowość do chmury (Cloud-Native) – w przyszłości projekt AppHost może posłużyć do automatycznego wygenerowania manifestów wdrożeniowych (np. dla Azure Container Apps lub Kubernetes poprzez narzędzie `Azd`).

### Negatywne (Wyzwania i ryzyka):

- **Wiązanie z ekosystemem .NET:** Narzędzie to jest najpotężniejsze, gdy mikroserwisy są pisane w C#/.NET. Choć Aspire potrafi uruchamiać kontenery Dockerowe i aplikacje Node.js (co pozwala na integrację z różnymi technologiami), jego główna siła i konfiguracja leży w świecie .NET.
- **Nowa warstwa abstrakcji:** Konieczność nauki modelu aplikacyjnego Aspire (projekty AppHost i ServiceDefaults) obok nauki samej architektury mikroserwisów.
