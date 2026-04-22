# RPCMAS — Retail Price Change & Markdown Approval System

Take-home submission for KCC Malls. Fullstack app for managing retail price change requests through an approval workflow (Draft → Submitted → Approved → Applied, with Reject / Cancel as terminal off-ramps).

**Author:** Keith Agreda (keithagreda123@gmail.com)

## Stack

- ASP.NET Core Web API (.NET 10) + Blazor Server (.NET 10)
- EF Core 10 + SQL Server 2022
- Redis 7 (read-through cache for items + request lists)
- NUnit + Moq (10 unit tests covering the workflow + business rules)
- Docker Compose

## Quickstart

```bash
docker compose up --build
```

| Service     | URL                                  |
|-------------|--------------------------------------|
| Blazor UI   | http://localhost:5090                |
| API (OpenAPI JSON) | http://localhost:5080/openapi/v1.json |
| SQL Server  | localhost:1433 (sa / `Your_strong_Pass123!`) |
| Redis       | localhost:6379                       |

API auto-runs EF migrations and seeds data on startup (5 departments, 3 roles, **>1000 items** generated combinatorially from spec products × brand × color × size).

Open http://localhost:5090, pick a user from the top-right dropdown (mock auth), then exercise the flow at `/items` and `/requests`.

## Folder Structure

```
RPCMAS/
├── src/
│   ├── RPCMAS.API/            controllers, validators, Program.cs, Dockerfile
│   ├── RPCMAS.Blazor/         pages, ApiClient, mock-auth picker, Dockerfile
│   ├── RPCMAS.Core/           entities, enums, interfaces (no infra deps)
│   └── RPCMAS.Infrastructure/ DbContext, repos, Redis cache, services, seeder
├── tests/RPCMAS.Tests/        NUnit + Moq + EF InMemory
├── docker-compose.yml
└── RPCMAS.slnx                XML solution file (.NET 10 format)
```

Dependency rule: `Core` has zero infra deps. `Infrastructure` implements interfaces declared in `Core`. `API` and `Blazor` never reference each other directly — Blazor talks to API via typed `HttpClient`.

## Architecture Notes

- **Auth:** mocked. Blazor exposes a user/role dropdown; selection is sent to the API in an `X-User-Id` header (via `DelegatingHandler`) and resolved server-side in `MockCurrentUserService`. Replace with real JWT/Identity later.
- **API versioning:** routes prefixed `/api/v1/...` via `Asp.Versioning.Mvc.ApiExplorer`.
- **Concurrency:** `RowVersion` (`[Timestamp] byte[]`) on both `Item` and `PriceChangeRequest`. Workflow transitions and `Apply` set `OriginalValue` on the row version → EF throws `DbUpdateConcurrencyException` on stale write → service maps to `ConcurrencyException` → controller maps to HTTP 409.
- **Atomic Apply:** `IExecutionStrategy.ExecuteAsync` wraps the price update + status flip in a single transaction (compatible with EF retry-on-failure).
- **Cache invalidation:** Apply blasts the `items:list:*`, `items:id:*`, and `requests:list:*` Redis prefixes via `IServer.Keys` scan.
- **Mutations invalidate the request-list cache prefix** so list pages stay fresh after Create/Submit/Approve/etc.

## Business Rules (enforced in service layer, see `PriceChangeRequestService`)

1. Request must contain ≥1 item.
2. Proposed price > 0.
3. Proposed price ≠ current price.
4. Only `Draft` is editable / submittable.
5. Only `Submitted` is approvable / rejectable.
6. Only `Approved` is applicable.
7. `Rejected` / `Cancelled` / `Applied` are terminal.
8. Apply atomically updates `Item.CurrentPrice` AND invalidates item + request caches.
9. Applied requests stay visible (history / audit — never hard-deleted).

Markdown %: `((CurrentPrice − ProposedNewPrice) / CurrentPrice) × 100`.

## Tests

```bash
dotnet test RPCMAS.slnx
```

10 NUnit tests in `tests/RPCMAS.Tests/PriceChangeRequestServiceTests.cs` cover: create validation (no items, zero price, equal price), full happy-path lifecycle (Draft → Submit → Approve → Apply with item-price mutation + cache invalidation verification), invalid status guards on Submit/Approve/Apply/Cancel, and Reject reason persistence. Built with EF InMemory + Moq for repos/cache/clock.

## Local Dev (without Docker)

```bash
dotnet restore RPCMAS.slnx
dotnet build RPCMAS.slnx
dotnet run --project src/RPCMAS.API
dotnet run --project src/RPCMAS.Blazor
```

Still needs SQL + Redis. Easiest: `docker compose up sqlserver redis`, then run the apps locally for hot reload.

EF migrations:

```bash
dotnet ef migrations add <Name> -p src/RPCMAS.Infrastructure -s src/RPCMAS.API
dotnet ef database update     -p src/RPCMAS.Infrastructure -s src/RPCMAS.API
```

## Configuration

API:
- `ConnectionStrings__Default` — SQL Server connection string
- `Redis__Connection` — `host:port`

Blazor:
- `ApiBaseUrl` — API base URL

Set via compose env in Docker, or `appsettings.Development.json` locally.

## Known Decisions / Deviations

- **Targeted .NET 10**, not the spec's .NET 8 — template default. Same APIs, just newer SDK.
- **`.slnx` solution file** (XML, .NET 10 format), not `.sln`.
- **Blazor Server**, not WASM (lower latency for internal tool, simpler auth bridge).
- **`UseHttpsRedirection` disabled in Development** — Docker exposes only HTTP:8080; redirect would 307 to a dead port.
- **No Swagger UI by default** — .NET 10 template ships `MapOpenApi()` (JSON only).
