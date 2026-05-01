# ADR 014: Standard vertical slices w projektach .NET

## 1. Status

**Zaakceptowany**

## 2. Kontekst

System Dark Kitchen rozwijamy jako zestaw mikroserwisów .NET, w których każdy bounded context ma własne API, bazę danych i kontrakty integracyjne. Wraz z rozwojem serwisów pojawia się ryzyko rozlewania kodu po katalogach technicznych typu `Endpoints`, `Services`, `Validators`, `Mappings` i `Repositories`, przez co pojedynczy przypadek użycia wymaga skakania po całym projekcie.

Jako punkt odniesienia przyjmujemy podejście opisane w artykule Antona Martyniuka [„The Best Way To Structure Your .NET Projects with Clean Architecture and Vertical Slices”](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices): domena i infrastruktura pozostają czytelnymi granicami, a warstwa application/presentation jest organizowana pionowo według funkcji.

## 3. Decyzja

Każdy bounded context serwisu .NET rozbijamy na cztery projekty:

```text
DarkKitchen.<Context>.Api
DarkKitchen.<Context>.Domain
DarkKitchen.<Context>.Features
DarkKitchen.<Context>.Infrastructure
```

`Api` jest cienkim composition rootem. Rejestruje host, ServiceDefaults, middleware i wywołuje metody mapujące endpointy. Nie zawiera implementacji use-case’ów.

`Domain` jest biblioteką klas bez zależności projektowych. Encje domenowe trzymamy po jednej klasie na plik. Stosujemy pragmatyczne DDD: fabryki i metody typu `Create`, `Update`, `Activate`, `Deactivate`, `Touch`, ale bez ciężkiej ceremonii, jeżeli model jej jeszcze nie potrzebuje.

`Features` jest biblioteką klas z vertical slices. Standard wewnątrz projektu:

```text
Application/
Features/
  <SliceName>/
    <UseCase>Endpoint.cs
    <UseCase>Handler.cs
    <UseCase>Validation.cs
    <UseCase>Mapping.cs
```

`DbContext` jest dozwolony w `Features/Application`. W tym projekcie traktujemy go jako aplikacyjną abstrakcję dostępu do danych używaną przez slice’y. Provider EF, design-time factory, inicjalizacja bazy, migracje runtime, zewnętrzne adaptery i techniczne integracje należą do `Infrastructure`.

`Infrastructure` jest biblioteką klas dla adapterów technicznych. Może referencjonować `Domain` i `Features`, ale nie referencjonuje `Api` ani `ServiceDefaults`.

Zależności projektów są jednokierunkowe:

```text
Api -> Features
Api -> Infrastructure
Api -> ServiceDefaults
Features -> Domain
Infrastructure -> Domain
Infrastructure -> Features, gdy potrzebuje typów aplikacyjnych, np. DbContext
Domain -> brak zależności
```

Solution Explorer grupujemy per serwis: `/src/Services/<Context>/`, a pod nim projekty `Api`, `Domain`, `Features`, `Infrastructure`.

Projekty wspierające, które nie reprezentują przypadków użycia API, takie jak `DarkKitchen.Contracts`, `DarkKitchen.ServiceDefaults`, `DarkKitchen.AppHost` i projekty testowe, zachowują strukturę wynikającą z ich roli. Nie wymuszamy w nich sztucznych vertical slices.

## 4. Uzasadnienie

- Zmiana konkretnego use-case’u ma być lokalna i łatwa do review.
- Endpoint, request/response DTO, walidacja, mapping i handler powinny być blisko siebie.
- Granice bounded contextów pozostają ważniejsze niż współdzielenie kodu między serwisami.
- Domena pozostaje czysta i niezależna od transportu.
- Infrastruktura nie rozlewa się po feature’ach, ale nie blokujemy bezpośredniego użycia EF Core tam, gdzie upraszcza slice.
- Standard ogranicza nadmiarowe abstrakcje typu repozytorium tworzone tylko po to, aby ukryć EF Core.

## 5. Konsekwencje

### Pozytywne

- Łatwiejsza nawigacja po kodzie konkretnej funkcji.
- Mniejsze ryzyko przypadkowego wpływu zmian na inne slice’y.
- Prostsze dodawanie kolejnych przypadków użycia w rosnących serwisach.
- Czytelny kompromis między Clean Architecture a Minimal APIs.
- Architektura jest możliwa do automatycznego sprawdzenia testami.

### Negatywne

- Może pojawić się kontrolowana duplikacja między slice’ami.
- Wymaga dyscypliny przy wydzielaniu naprawdę wspólnych mechanizmów.
- Małe serwisy będą miały więcej projektów niż wariant z endpointami w `Program.cs`.
- Trzeba pilnować, żeby `Application` nie stało się katalogiem na wszystko.

## 6. Zasady implementacyjne

1. Nowy endpoint dodajemy w projekcie `DarkKitchen.<Context>.Features`, w katalogu `Features/<SliceName>`.
2. DTO wejściowe i wyjściowe trzymamy przy endpointzie lub handlerze danego slice’a.
3. Walidacja wejścia ma być blisko granicy API, zwykle w tym samym slice’ie.
4. Mapping utrzymujemy jawnie jako extension methods albo małe metody fabrykujące, zgodnie z ADR 012.
5. Kod współdzielony między slice’ami przenosimy do `Features/Application` tylko wtedy, gdy ma realne ponowne użycie.
6. Encje i reguły domenowe trafiają do projektu `Domain`; jedna encja to jeden plik.
7. Encje nie powinny być workiem publicznych setterów. Preferujemy fabryki i metody zmiany stanu, ale bez nadmiarowych abstrakcji.
8. `DbContext` może być w `Features/Application`; provider, factory, migracje i runtime database bootstrap trafiają do `Infrastructure`.
9. Integracje zewnętrzne, messaging adaptery, cache i techniczne klienty należą do `Infrastructure`.
10. `Program.cs` nie zawiera mapowania pojedynczych endpointów poza wywołaniem metod rejestrujących slice’y.
11. Odstępstwo od standardu wymaga uzasadnienia w PR albo nowego ADR.

## 7. Egzekwowanie

Dodajemy testy architektoniczne w `tests/DarkKitchen.ArchitectureTests`. Testy wymagają projektów `Api`, `Domain`, `Features` i `Infrastructure` w każdym bounded context, pilnują kierunku referencji oraz blokują umieszczanie kodu use-case’ów w projekcie `Api`.
