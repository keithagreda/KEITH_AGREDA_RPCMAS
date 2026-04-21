# RPCMAS — Retail Price Change & Markdown Approval System

Fullstack app for managing retail price change requests with approval workflow.

## Stack

- ASP.NET Core Web API (.NET 10)
- Blazor (Server)
- EF Core + SQL Server 2022
- Redis 7
- NUnit + Moq
- Docker Compose

## Folder Structure

```
RPCMAS/
├── src/
│   ├── RPCMAS.API/            # Web API — controllers, DTOs, Program.cs
│   ├── RPCMAS.Blazor/         # Blazor frontend
│   ├── RPCMAS.Core/           # Entities, enums, interfaces
│   └── RPCMAS.Infrastructure/ # EF Core DbContext, repos, Redis cache, services
├── tests/
│   └── RPCMAS.Tests/          # NUnit + Moq
├── docker-compose.yml
├── RPCMAS.slnx
└── README.md
```

Project reference flow: `API → Infrastructure → Core`. `Blazor → Core`. `Tests → all`.

## Run with Docker

```bash
docker compose up --build
```

| Service   | URL                            |
|-----------|--------------------------------|
| Blazor    | http://localhost:5090          |
| API       | http://localhost:5080/swagger  |
| SQL Server| localhost:1433 (sa / Your_strong_Pass123!) |
| Redis     | localhost:6379                 |

API auto-runs EF migrations and seeds data on startup.

## Local Dev (without Docker)

```bash
dotnet restore
dotnet build
dotnet run --project src/RPCMAS.API
dotnet run --project src/RPCMAS.Blazor
dotnet test
```

You still need SQL Server + Redis running locally. Easiest: `docker compose up sqlserver redis`.

## Configuration

API reads:
- `ConnectionStrings__Default` — SQL Server connection
- `Redis__Connection` — Redis host:port

Blazor reads:
- `ApiBaseUrl` — base URL for the API client
