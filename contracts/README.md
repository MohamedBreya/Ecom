# Frontend contracts

| File | Purpose |
|---|---|
| [`openapi.json`](openapi.json) | OpenAPI 3 document generated from the running API. Source of truth. Feed it to a generator (`openapi-typescript`, NSwag, Orval, ...) if you prefer generated code. |
| [`ts/types.ts`](ts/types.ts) | Hand-written TypeScript types: response envelopes, `Category`, `Product`, inputs. |
| [`ts/client.ts`](ts/client.ts) | Small typed client built on `fetch` (no dependencies), usable from any framework. |

## Using the client

Copy `contracts/ts/` into your frontend (or publish it as a package) and import from `index.ts`:

```ts
import { createApiClient, ApiError } from "./ecom-api";

const api = createApiClient({ baseUrl: "http://localhost:5159" });

const products = await api.products.getAll();            // Product[]
const src = api.imageUrl(products[0].photos[0].imageName); // absolute URL for <img>

try {
  await api.products.create({
    name: "Phone X",
    description: "A very good phone",
    newPrice: 499.99,
    oldPrice: 599.99,
    categoryId: 1,
    photos: fileInput.files ? Array.from(fileInput.files) : [],
  });
} catch (e) {
  if (e instanceof ApiError && e.isValidationError) {
    console.log(e.errors); // { name: ["..."], newPrice: ["..."] }
  } else {
    throw e;
  }
}
```

Methods:

- `api.categories`: `getAll()`, `getById(id)`, `create(input)`, `update(id, input)`, `remove(id)`
- `api.products`: `getAll()`, `getById(id)`, `create(input)`, `update(id, input)`, `remove(id)`
- `api.imageUrl(imageName)`

Data methods return the unwrapped `data`; `remove` returns the `{ statusCode, message }` envelope. Every non-2xx response throws an `ApiError` carrying `statusCode`, `message`, and (for validation failures) `errors`.

Options: `fetch` to inject a custom implementation, `headers` for things like an `Authorization` token (the API has no authentication yet).

Requires a runtime with `fetch`, `FormData` and `Blob` (browsers, Node 18+). The API must list your frontend's origin in `Cors:AllowedOrigins`.

## Keeping the contract in sync

Regenerate `openapi.json` whenever endpoints or DTOs change, then update `ts/types.ts` to match:

```bash
dotnet run --project Ecom.API --launch-profile http   # in one terminal
curl -s http://localhost:5159/swagger/v1/swagger.json | python3 -m json.tool > contracts/openapi.json
```

Type-check the client:

```bash
cd contracts/ts && npx -p typescript tsc -p tsconfig.json
```
