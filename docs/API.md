# API reference

Base URL (development): `http://localhost:5159`. Interactive docs: `/swagger`. Machine-readable spec: [`contracts/openapi.json`](../contracts/openapi.json).

- All request and response bodies are JSON (`application/json`) except product create/update, which are `multipart/form-data`.
- JSON property names are **camelCase**.
- There is no authentication yet.

## Response envelope

Every response uses the same shape, so the frontend can handle results uniformly.

**Success with data**

```json
{ "statusCode": 200, "message": "Success", "data": { "...": "..." } }
```

**Success without data** (deletes)

```json
{ "statusCode": 200, "message": "Category deleted." }
```

**Error** (any non-2xx status)

```json
{ "statusCode": 404, "message": "Category with id 999 was not found." }
```

Validation failures add an `errors` object keyed by camelCase field name:

```json
{
  "statusCode": 400,
  "message": "Validation failed.",
  "errors": { "name": ["The Name field is required."] }
}
```

In the Development environment, 500 responses also include `details` (the stack trace). It is never sent in other environments, and in those the 500 message is a generic "An unexpected error occurred."

### Status codes

| Code | When |
|---|---|
| 200 | Read, update or delete succeeded |
| 201 | Created. The `Location` header points at the new resource. |
| 400 | Validation failed, an image is not allowed, or a referenced category does not exist |
| 404 | The resource or the route does not exist (also returned for a non-numeric id such as `/api/categories/abc`) |
| 405 | Wrong HTTP method for the route |
| 409 | Category cannot be deleted because it still has products |
| 429 | Rate limit exceeded. `Retry-After` gives the seconds to wait. |
| 500 | Unexpected server error |

## Categories

`CategoryResponse`: `{ "id": 1, "name": "Phones", "description": "Smartphones" }`

| Method | Route | Body | Success |
|---|---|---|---|
| GET | `/api/categories` | none | 200, `data`: `CategoryResponse[]` |
| GET | `/api/categories/{id}` | none | 200, `data`: `CategoryResponse` |
| POST | `/api/categories` | `CategoryInput` | 201, `data`: `CategoryResponse` |
| PUT | `/api/categories/{id}` | `CategoryInput` | 200, `data`: `CategoryResponse` |
| DELETE | `/api/categories/{id}` | none | 200, no `data` |

`CategoryInput` (JSON):

| Field | Rules |
|---|---|
| `name` | required, max 30 characters |
| `description` | required, max 500 characters |

Errors: `404` unknown id (GET one, PUT, DELETE); `409` on DELETE while the category has products; `400` on validation.

```bash
curl -X POST http://localhost:5159/api/categories \
  -H 'Content-Type: application/json' \
  -d '{"name":"Phones","description":"Smartphones"}'
```

## Products

`ProductResponse`:

```json
{
  "id": 2,
  "name": "Phone X",
  "description": "A very good phone",
  "newPrice": 499.99,
  "oldPrice": 599.99,
  "categoryId": 1,
  "categoryName": "Phones",
  "photos": [{ "id": 6, "imageName": "Images/Phone-X/d349c0fc....png" }]
}
```

| Method | Route | Body | Success |
|---|---|---|---|
| GET | `/api/products` | none | 200, `data`: `ProductResponse[]` |
| GET | `/api/products/{id}` | none | 200, `data`: `ProductResponse` |
| POST | `/api/products` | multipart form (below) | 201, `data`: `ProductResponse` |
| PUT | `/api/products/{id}` | multipart form (below) | 200, `data`: `ProductResponse` |
| DELETE | `/api/products/{id}` | none | 200, no `data` (the product's image files are deleted too) |

Form fields for POST and PUT (field names are case-insensitive):

| Field | Type | Rules |
|---|---|---|
| `Name` | text | required, max 100 characters |
| `Description` | text | required, max 2000 characters |
| `NewPrice` | decimal | at least 0.01 |
| `OldPrice` | decimal | 0 or more |
| `CategoryId` | integer | must reference an existing category, otherwise 400 |
| `Photos` | file, repeatable | optional, see [Images](#images) |

**Update semantics:** PUT replaces all scalar fields. If `Photos` is sent, the existing photos are replaced by the new ones; if it is omitted, the existing photos are kept.

```bash
curl -X POST http://localhost:5159/api/products \
  -F Name="Phone X" -F Description="A very good phone" \
  -F NewPrice=499.99 -F OldPrice=599.99 -F CategoryId=1 \
  -F "Photos=@front.png" -F "Photos=@back.png"
```

### Images

- Allowed: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, with an `image/*` content type. Anything else returns 400.
- Maximum 5 MB per file.
- Files are stored under a random name; the client's file name is never used on disk. The folder name is derived from the product name with unsafe characters replaced.
- `photos[].imageName` is a path relative to the API root. The full URL is `{baseUrl}/{imageName}`, for example `http://localhost:5159/Images/Phone-X/d349c0fc....png`.
- If saving the product fails, images already written for that request are removed.
- Limitation: the type check uses the extension and the declared content type, not the file's magic bytes.

## Cross-cutting behaviour

- **CORS:** only origins in `Cors:AllowedOrigins` may call the API from a browser.
- **Rate limit:** 100 requests per minute per client IP by default (`RateLimiting:*`). Exceeding it returns 429 with `Retry-After`.
- **Security headers** on every response: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`.
- **Dev-only diagnostics:** `/api/bug/*` routes exist only to exercise error handling, respond only in Development, and are excluded from the OpenAPI document.
