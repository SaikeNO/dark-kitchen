# ADR 012: Rezygnacja z AutoMappera na rzecz jawnych mapperów jako extension methods

## 1. Status

**Zaakceptowany**

## 2. Kontekst

W systemie powstaje wiele prostych i średnio złożonych mapowań pomiędzy:

- encjami i modelami domenowymi,
- modelami zapisu i odczytu,
- kontraktami API i wewnętrznymi DTO,
- zdarzeniami integracyjnymi i modelami aplikacyjnymi.

Możemy rozwiązać ten problem przez bibliotekę typu AutoMapper albo przez jawnie napisane mapowania utrzymywane w kodzie aplikacji.

## 3. Decyzja

Nie używamy AutoMappera w tym rozwiązaniu.

Zamiast tego tworzymy własne, jawne mapowania w postaci statycznych klas z metodami rozszerzającymi (`extension methods`), np.:

- `OrderEntityExtensions`
- `OrderDtoExtensions`
- `CatalogItemMappingExtensions`

Każdy moduł lub serwis posiada własny zestaw mapperów, umieszczony blisko modelu, którego dotyczy.

## 4. Uzasadnienie

### Czytelność i kontrola

Jawny kod mapujący pokazuje dokładnie, co jest kopiowane i transformowane. Nie ukrywa reguł pod konfiguracją lub refleksją, więc łatwiej go prześledzić podczas debugowania.

### Bezpieczeństwo refaktoryzacji

Przy ręcznie zapisanych mapowaniach kompilator szybciej wykrywa zmiany nazw pól, typów i kontraktów. To ogranicza ryzyko cichych błędów po refaktorze.

### Mniej magii

AutoMapper upraszcza proste przypadki, ale w praktyce wprowadza dodatkową warstwę abstrakcji i konwencji, które trzeba znać, aby zrozumieć przepływ danych. W tym projekcie chcemy, żeby mapping był bezpośredni i przewidywalny.

### Lepsze dopasowanie do domeny

W wielu miejscach mapowanie nie jest 1:1 i wymaga decyzji domenowych. Własne metody rozszerzające pozwalają umieścić takie reguły obok konkretnego modelu, bez rozlewania logiki po konfiguracji globalnej.

### Mniejszy narzut zależnościowy

Rezygnacja z AutoMappera upraszcza zależności, konfigurację i start aplikacji. To ważne w rozwiązaniu z wieloma serwisami i lokalną orkiestracją przez Aspire.

## 5. Konsekwencje

### Pozytywne

- Mapowania są jawne i łatwe do debugowania.
- Zmiany kontraktów są szybciej widoczne w kompilacji.
- Logika transformacji pozostaje w kodzie serwisu, a nie w zewnętrznej konfiguracji.
- Łatwiej pisać testy jednostkowe dla konkretnych mapowań.

### Negatywne

- Więcej kodu do utrzymania niż w przypadku automatycznego mapera.
- Trzeba świadomie pilnować spójności nazw i kierunków mapowań.
- W dużej liczbie modeli może pojawić się powtarzalność, jeśli nie utrzymamy porządku w strukturze mapperów.

## 6. Zasady implementacyjne

1. Mapowania zapisujemy jako `static` extension methods.
2. Dla jednego agregatu lub kontraktu utrzymujemy jeden spójny plik z mapperami.
3. Metody mapujące mają być małe i czytelne, bez ukrytej logiki biznesowej.
4. Transformacje wykraczające poza proste kopiowanie pól wydzielamy do jawnych metod pomocniczych.
5. Jeśli mapowanie staje się skomplikowane, nie wracamy do AutoMappera, tylko rozbijamy je na mniejsze kroki lub osobne klasy pomocnicze.

