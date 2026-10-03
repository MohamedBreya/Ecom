# Refactoring notes

Work done on branch `refactor/cleanup-docs-api-contracts`. This page lists what changed, what breaks for existing callers, and how it was verified.

## Bugs fixed

| Problem | Fix |
|---|---|
| Rate limiter wrote a 429 and then still ran the rest of the pipeline | Replaced by the built-in ASP.NET Core rate limiter (`AddApiRateLimiting`) |
| `ProductDTO.Price` mapped to nothing, so every product showed price 0; no `Id` in product responses | DTOs now expose `id`, `newPrice`, `oldPrice`, `categoryId`, `categoryName`, `photos` |
| Migrations never matched the model: `Price decimal(18,0)` was created and the `updatepricesuccess` migration was empty, while the model uses `NewPrice`/`OldPrice` and `"decimal(18.2)"` | New idempotent migration `FixPriceColumns`; model type corrected to `decimal(18,2)` |
| Deleting or updating a missing product/category threw a null reference (500) | 404 responses |
| Missing resources returned 400 | 404 |
| Updating a product without photos crashed and deleted its old photos first | Photos are optional; existing ones are kept unless new ones are sent, and old files are removed only after the save succeeds |
| Uploaded images were never reachable (`UseStaticFiles` missing) | Static files enabled |
| App crashed at startup on a fresh clone because `wwwroot` did not exist | `ImageManagementService` creates folders as needed; `wwwroot/Images/.gitkeep` is tracked |
| Adding a product saved twice, leaving a photo-less product if image saving failed | Single save; image files are cleaned up on failure |
| Deleting a category cascaded to its products and left their image files behind | Delete is refused with 409 while products exist |
| Unknown `CategoryId` caused a foreign key exception (500) | 400 with a clear message |

## Security

- **Path traversal on upload:** client-supplied file names and the product name were used as paths. Files now get random names and the folder name is sanitised. `DeleteImage` refuses paths outside the web root.
- **Upload validation:** extension allow-list, `image/*` content type, 5 MB limit.
- **Information leak:** production responses no longer include exception messages; stack traces appear only in Development. Controllers no longer return raw `ex.Message`.
- **AutoMapper 15.0.1 to 15.1.1:** 15.0.1 has a known high-severity denial-of-service advisory (GHSA-rvv3-g6hj-g44x). 15.1.1 is the patched release.
- **Headers:** removed the deprecated `X-XSS-Protection`, added `Referrer-Policy`.
- **CORS** is now explicit and configuration-driven instead of absent.

## Structure and cleanup

- Controllers are thin: no `try/catch`, no entities returned directly, shared envelope, exceptions mapped by one middleware.
- Middleware split into `ExceptionMiddleware` and `SecurityHeadersMiddleware`; rate limiting, CORS and validation-response setup moved to `Extensions/ApiServiceCollectionExtensions.cs`.
- Repositories are registered in DI; `UnitOfWork` receives them instead of calling `new`. `IGenericRepository<T>` is constrained to `BaseEntity<int>` (no more `EF.Property` string lookup) and its duplicate overloads were merged into `params` includes.
- `Ecom.core` no longer references the deprecated `Microsoft.AspNetCore.Http.Features 5.0.17` package; it uses the shared framework. The unused `Microsoft.Extensions.FileProviders.*` packages and the `IFileProvider` registration were removed.
- Typos fixed in names: `Configration` to `Configuration`, `Registeration` to `Registration`, `Managment` to `Management`, `infrastructureConfigration` to `AddInfrastructure`.
- Commented-out code and stale files removed (`weatherforecast` in the `.http` file, `Helper/` folder replaced by `Models/ApiResponse.cs`).
- `BugController` kept for error-handling demos but now Development-only and hidden from OpenAPI.

## Breaking changes for existing callers

**Routes**

| Before | After |
|---|---|
| `GET /api/Categories/get-all` | `GET /api/categories` |
| `GET /api/Categories/get-by-id/{id}` | `GET /api/categories/{id}` |
| `POST /api/Categories/add-category` | `POST /api/categories` |
| `PUT /api/Categories/update-category` (id in body) | `PUT /api/categories/{id}` |
| `DELETE /api/Categories/delete-category/{id}` | `DELETE /api/categories/{id}` |
| `GET /api/Product/get-all` | `GET /api/products` |
| `GET /api/Product/get-by-id/{id}` | `GET /api/products/{id}` |
| `POST /api/Product/add-product` | `POST /api/products` |
| `PUT /api/Product/update-product` (id in body) | `PUT /api/products/{id}` |
| `DELETE /api/Product/delete-product/{id}` | `DELETE /api/products/{id}` |

**Payloads**

- All successful responses are wrapped: `{ statusCode, message, data }`. Previously lists/objects were returned bare.
- Product form file field renamed `Photo` to `Photos`. Product update no longer takes `Id` in the body.
- `UpdateCategoryDTO` removed; create and update share `CategoryDTO`.
- Product responses: `price` replaced by `newPrice`/`oldPrice`; `id`, `categoryId` added; `photos[].productId` removed, `photos[].id` added.
- Errors use one envelope with proper status codes (404, 409, 429) instead of 400 for everything.
- Rate limit default is now 100 requests per 60 s per IP (it was 8 per 30 s). Configurable.
- Stored image paths now use random file names. Existing rows are untouched.

## Database migration

Run `dotnet ef database update` as usual. `FixPriceColumns` is safe on both a fresh database and one already patched by hand:

1. renames `Products.Price` to `NewPrice` if only the old column exists;
2. adds `OldPrice decimal(18,2)` if missing;
3. alters the price columns to `decimal(18,2)` only when they are not already that type.

It has no `Down` step, on purpose. If you previously altered the table manually, check the result with `SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products'`.

## How it was verified

There is no test project, so verification was manual, against SQL Server 2022 in Docker:

- `dotnet build` of the solution: 0 errors.
- `dotnet ef migrations has-pending-model-changes`: none, so the snapshot matches the model.
- `dotnet ef database update` on an empty database: all migrations apply; `Products` ends with `NewPrice`/`OldPrice decimal(18,2)`.
- Every endpoint exercised with curl, including: validation errors, 404, 405, 409, 429 with `Retry-After`, 500 envelope, image upload, traversal file name and product name, disguised and oversized files, update with and without photos, delete cleaning up files and folders, and static serving of an uploaded image.
- The TypeScript client was type-checked with `tsc --strict` and run against the live API (create, upload, update, validation/409/404 errors, delete).

## Not done / suggested next steps

- Automated tests (unit tests for repositories and the image service; integration tests with `WebApplicationFactory`).
- Authentication and authorization.
- Pagination, search and sorting for products.
- Product photos on blob storage; magic-byte image validation.
- Forwarded-headers configuration if deployed behind a proxy.
- Considering whether to keep AutoMapper (commercial license for production) or switch to hand-written mapping.
- The pre-existing `init`/`updatepricesuccess` migration class names trigger lower-case-name compiler warnings (CS8981). They were left alone because renaming applied migrations is unsafe.
