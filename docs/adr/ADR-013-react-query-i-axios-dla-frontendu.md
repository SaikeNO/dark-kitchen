# ADR 013: React Query i Axios jako standard komunikacji frontendow

## 1. Status

**Zaakceptowany**

## 2. Kontekst

Frontend w tym projekcie nie jest tylko warstwą prezentacji. Obsługuje kilka aplikacji operacyjnych i sprzedażowych, które pobierają dane z API, wykonują mutacje, odświeżają widoki po zmianach i muszą dobrze znosić opóźnienia sieci, retry oraz chwilową niespójność danych.

Musimy zdecydować:

- czy zarządzanie danymi serwerowymi robimy ręcznie przez `useEffect` i `fetch`,
- czy używamy dedykowanego mechanizmu do server state,
- oraz jaką biblioteką obsługujemy wywołania HTTP.

## 3. Decyzja

W aplikacjach frontendowych stosujemy:

- **TanStack Query** do zarządzania stanem serwerowym,
- **Axios** jako standardowy klient HTTP.

Nie używamy `useEffect` jako domyślnego mechanizmu pobierania danych z API. `useEffect` zostaje do efektów ubocznych, które naprawdę są związane z cyklem życia komponentu albo integracją z zewnętrznym systemem.

## 4. Uzasadnienie

### TanStack Query pasuje do server state

TanStack Query daje cache, deduplikację żądań, refetch po ponownym montażu, ponowne pobranie po powrocie do okna i kontrolę nad świeżością danych. To jest dokładnie problem, który mamy w frontendzie opartym o dane z backendu, a nie lokalny stan UI.

### Query key jako kontrakt

Klucze zapytań są jawne, serializowalne i powinny uwzględniać wszystkie parametry wejściowe. To upraszcza cache i ogranicza błędy typu "ten sam hook, inne dane".

### Mutacje przez invalidację, nie ręczne sklejanie cache

Po udanej mutacji najbezpieczniej jest oznaczyć powiązane query jako nieaktualne i pozwolić Query Clientowi pobrać je ponownie. To jest prostsze i mniej kruche niż ręczne przepisywanie wielu fragmentów cache.

### Axios jako cienka warstwa transportowa

Axios dobrze nadaje się do wspólnej konfiguracji `baseURL`, timeoutów, nagłówków i interceptorów. W tym projekcie nie traktujemy go jako warstwy stanu, tylko jako transport HTTP.

### Lepsze niż ręczne fetchowanie w `useEffect`

React zaleca używanie zewnętrznych bibliotek do pobierania danych, jeśli aplikacja ma bardziej niż trywialne potrzeby. W naszym przypadku ręczne `fetch` w `useEffect` szybko prowadzi do duplikacji logiki, wyścigów, stanów ładowania rozsianych po komponentach i słabej obsługi ponownego pobierania.

## 5. Dobre praktyki

1. Używamy jednego `QueryClient` na aplikację i ustawiamy go centralnie.
2. Query keys zapisujemy jako tablice, np. `['menu', brandId]` albo `['order', orderId]`.
3. Jeśli query zależy od parametru, parametr musi być częścią query key.
4. Dla danych często oglądanych ustawiamy sensowny `staleTime`, zamiast polegać na domyślnym natychmiastowym uznaniu danych za nieaktualne.
5. Po mutacjach invalidujemy powiązane query przez `queryClient.invalidateQueries(...)`.
6. Axios konfigurujemy przez `axios.create(...)`, a nie przez globalne mutowanie domyślnych ustawień.
7. Do przechwytywania nagłówków autoryzacji, `BrandId` lub wspólnych błędów używamy interceptorów na instancji, nie w każdym komponencie osobno.
8. Do anulowania żądań używamy `AbortController` i `signal`, a nie przestarzałego `CancelToken`.
9. Nie duplikujemy danych z API do lokalnego stanu komponentu, jeśli może nimi zarządzać TanStack Query.

## 6. Przykład

Poniższy przykład pokazuje standardową konfigurację klienta HTTP oraz pobieranie menu z cache i invalidacją po mutacji:

```tsx
// api/client.ts
import axios from "axios";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10_000,
});

api.interceptors.request.use((config) => {
  config.headers = config.headers ?? {};
  config.headers["X-Brand-Id"] = localStorage.getItem("brandId") ?? "";
  return config;
});
```

```tsx
// features/menu/useMenu.ts
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../api/client";

type MenuItem = {
  id: string;
  name: string;
  price: number;
};

async function fetchMenu(brandId: string, signal?: AbortSignal): Promise<MenuItem[]> {
  const response = await api.get<MenuItem[]>("/menu", {
    params: { brandId },
    signal,
  });

  return response.data;
}

async function addToCart(itemId: string) {
  await api.post("/cart/items", { itemId });
}

export function useMenu(brandId: string) {
  return useQuery({
    queryKey: ["menu", brandId],
    queryFn: ({ signal }) => fetchMenu(brandId, signal),
    staleTime: 2 * 60 * 1000,
  });
}

export function useAddToCart() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: addToCart,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["menu"] });
    },
  });
}
```

## 7. Konsekwencje

### Pozytywne

- Spójny standard dostępu do danych we wszystkich frontendach.
- Mniej kodu do ręcznego zarządzania loadingiem, cache i błędami.
- Łatwiejsze odświeżanie danych po mutacjach.
- Lepsza kontrola nad requestami, timeoutami i anulowaniem.

### Negatywne

- Dochodzi kolejna warstwa abstrakcji, którą zespół musi znać.
- Trzeba pilnować poprawnych query keys i invalidacji.
- Źle ustawiony `staleTime` może ukryć zbyt świeże zmiany albo wywołać nadmiarowe refetches.

## 8. Zakres stosowania

1. Każda aplikacja frontendowa korzysta z TanStack Query do danych z backendu.
2. Axios służy do komunikacji z API HTTP, w tym z BFF i serwisami domenowymi.
3. Dla prostych, lokalnych stanów UI nadal używamy zwykłego stanu komponentu, jeśli nie dotyczy on serwera.
4. Jeśli backend wymaga specyficznego kontraktu albo mapowania błędów, rozwiązujemy to w warstwie klienta API, nie w komponentach.
