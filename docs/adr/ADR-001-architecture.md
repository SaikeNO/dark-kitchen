# ADR 001: Wybór Architektury Mikroserwisów dla Systemu Zarządzania Dark Kitchen

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Projektujemy system do zarządzania wirtualną kuchnią (Dark Kitchen). System musi obsługiwać cykl życia zamówienia, dekompozycję zamówień na pojedyncze zadania dla różnych stacji roboczych (grill, frytownica itp.), zarządzanie stanami magazynowymi w czasie rzeczywistym oraz agregację elementów podczas pakowania.

Z biznesowego i operacyjnego punktu widzenia, kuchnia to środowisko o dużej dynamice, w którym poszczególne stacje działają niezależnie i asynchronicznie, ale muszą ze sobą współpracować. Dodatkowo, jednym z głównych celów projektu jest edukacja w zakresie budowy systemów rozproszonych i komunikacji opartej na zdarzeniach (Event-Driven Architecture).

Zbudowanie tego systemu jako monolitu ułatwiłoby początkowy rozwój, ale nie odzwierciedlałoby fizycznej natury procesów na kuchni i uniemożliwiłoby realizację celu edukacyjnego.

## 3. Decyzja

Decydujemy się na zastosowanie **architektury mikroserwisów** (Microservices Architecture). System zostanie podzielony na niezależne usługi, odpowiadające konkretnym domenom biznesowym (np. Order Service, Inventory Service, KDS Service, Packing Service). Komunikacja między serwisami będzie odbywać się głównie asynchronicznie przy użyciu brokera wiadomości (Message Broker), z zachowaniem zasady _Database per Service_.

## 4. Uzasadnienie

- **Odzwierciedlenie domeny:** Mikroserwisy naturalnie mapują się na fizyczne stanowiska w kuchni. Tak jak stacja grilla nie musi wiedzieć, jak działa stacja pakowania (reaguje tylko na "zamówienie na burgera"), tak KDS Service nie musi znać logiki Order Service.
- **Cel edukacyjny:** Projekt ma służyć praktycznej nauce architektury rozproszonej, wzorców integracyjnych oraz rozwiązywania problemów ze spójnością ostateczną (eventual consistency).
- **Skalowalność:** W godzinach szczytu (np. piątek wieczór) system musi przetwarzać ogromną liczbę zdarzeń związanych z samą kuchnią (KDS Service). Mikroserwisy pozwolą nam skalować tylko te komponenty, które są pod największym obciążeniem, zamiast duplikować całą aplikację.
- **Izolacja błędów:** Awaria serwisu magazynowego (Inventory Service) nie zatrzyma możliwości przyjmowania nowych zamówień (Order Service) ani procesu smażenia frytek (KDS Service) – system będzie mógł wznowić aktualizację stanów po przywróceniu usługi.

## 5. Konsekwencje

### Pozytywne (Zalety):

- Zgodność z fizycznym modelem działania kuchni.
- Możliwość niezależnego wdrażania (deployments) poszczególnych serwisów.
- Swoboda w doborze technologii (np. Node.js dla KDS ze względu na WebSockety, Java/.NET dla logiki biznesowej zamówień).
- Znaczny przyrost wiedzy z zakresu systemów rozproszonych.

### Negatywne (Wyzwania i ryzyka):

- **Większa złożoność operacyjna:** Konieczność wdrożenia infrastruktury do logowania rozproszonego (np. ELK), monitoringu i CI/CD.
- **Złożoność komunikacji:** Trudniejsze debugowanie błędów przechodzących przez wiele serwisów w porównaniu do wywołania lokalnej funkcji w monolicie.
- **Zarządzanie danymi:** Brak możliwości korzystania ze standardowych transakcji bazodanowych (ACID) między serwisami, co wymusza stosowanie wzorców takich jak _Saga_ w przypadku niepowodzeń (np. anulowanie zamówienia i zwrot składników na magazyn).
