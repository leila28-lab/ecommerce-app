# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Context

.NET 10 e-commerce app (microservices + Blazor UI) orchestrated by .NET Aspire. It is the running example of a "Claude Code for Dev & DevOps" training course and is **intentionally bare**: no tests, no real database, no Dockerfile, no manifests, no CI. Those are meant to be added during the labs. The user-facing README is in French; each project under `src/` also has a short English `README.md` describing its files.

The only Claude config so far is the `/review-diff` command ([.claude/commands/review-diff.md](.claude/commands/review-diff.md)): a read-only review against `main` that needs a git repo. Its checklist sets the team conventions:
- When a route, DTO or JSON field changes, find who consumes it.
- Every behavior change needs a test.
- No hard-coded URLs, no secrets, no customer data in logs.
- Flag missing or destructive DB migrations.

## Commands

```bash
dotnet build ECommerce.slnx                 # build all 6 projects
dotnet run --project src/ECommerce.AppHost  # run everything (the AppHost is the ONLY project to launch)
dotnet dev-certs https --trust              # one-time; otherwise the Aspire dashboard fails with UntrustedRoot
```

- Running the AppHost prints a dashboard `Login URL` (with a `?t=` token). Ports change on every run, so never hard-code them. Stop with Ctrl+C. Docker is not required.
- There is no test project and no lint config yet.
- On the macOS training machine the SDK is in `/usr/local/share/dotnet`, which is not on PATH (`export PATH="/usr/local/share/dotnet:$PATH"`). This does not apply on Windows.
- `dotnet build` shows NU1902/NU1903 vulnerability warnings (`Microsoft.OpenApi` 2.0.0 in Catalog/Ordering, transitive `MessagePack` in AppHost). They don't fail the build.

## Git conventions

- Branches: `feat/`, `fix/`, `chore/`, `test/`, `ci/`. Never work directly on `main`.
- Commits: Conventional Commits, in English, imperative mood. Scopes: `catalog`, `ordering`, `gateway`, `web`, `db`, `ci` (e.g. `feat(ordering): validate order quantity`).

## Architecture

Request flow: **Web (Blazor Server) → Gateway (YARP) → Catalog.Api / Ordering.Api**, and **Ordering.Api → Catalog.Api** directly.

- **Service discovery by name, never URLs.** Resource names are declared in [src/ECommerce.AppHost/AppHost.cs](src/ECommerce.AppHost/AppHost.cs) (`catalog`, `ordering`, `gateway`, `web`) and used as `https+http://<name>` base addresses in typed `HttpClient`s and in the YARP cluster config. Adding a service or dependency means adding `AddProject` + `WithReference`/`WaitFor` in AppHost.cs.
- **Gateway routing** is configuration-only in [src/ECommerce.Gateway/appsettings.json](src/ECommerce.Gateway/appsettings.json): `/catalog/{**}` and `/ordering/{**}` with `PathRemovePrefix`. The services themselves expose `/api/products` and `/api/orders` with no prefix. The Web clients ([src/ECommerce.Web/Services/](src/ECommerce.Web/Services/)) call prefixed paths through the gateway. Ordering's `CatalogServiceClient` calls Catalog directly (unprefixed), bypassing the gateway.
- **ServiceDefaults** ([src/ECommerce.ServiceDefaults/Extensions.cs](src/ECommerce.ServiceDefaults/Extensions.cs)): every service calls `builder.AddServiceDefaults()` + `app.MapDefaultEndpoints()`. This applies OpenTelemetry, `/health` + `/alive` (Development only), service discovery, and `AddStandardResilienceHandler()` to **every** HttpClient, including retries on non-idempotent POSTs.
- **APIs** use minimal APIs grouped in `Endpoints/*Endpoints.cs` extension methods (`MapCatalogEndpoints`, `MapOrderingEndpoints`). There are no controllers. OpenAPI is at `/openapi/v1.json` in Development.
- **Data**: each API owns its own EF Core **InMemory** database (`UseInMemoryDatabase("catalog"|"ordering")`), created with `EnsureCreated()` at startup. Catalog seed products are in `CatalogDbContext.OnModelCreating` via `HasData`. All data is lost on restart.
- **No shared contracts project.** DTOs are duplicated per consumer (`CatalogProduct` in Ordering, `ProductDto`/order DTOs in Web), so a model change in one service must be mirrored by hand in its consumers.
- **Order creation** (`POST /api/orders`): validates each line by calling Catalog one product at a time, snapshots name/price into `OrderItem`, then saves as `Confirmed`. The only validation is "at least one item" and "the product exists" (400 otherwise). Quantity and customer are not validated, and stock is not checked or decremented. Ordering serializes enums as strings (`JsonStringEnumConverter`).
- **Web** is Blazor with interactive server rendering. Pages are in `Components/Pages/` (`Products.razor` holds catalog + cart + place order, `Orders.razor` the history).
