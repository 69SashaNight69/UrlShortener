# URL Shortener — INFORCE Test Task

A full-stack URL Shortener built with ASP.NET Core (backend + REST API + Razor Page) and Angular (SPA table UI), implementing authentication, role-based authorization, and the core shortening workflow.

## Table of Contents

- [Technologies](#technologies)
- [Architecture](#architecture)
- [Implemented Functionality](#implemented-functionality)
- [Roles & Permissions](#roles--permissions)
- [URL Shortening Algorithm](#url-shortening-algorithm)
- [Database](#database)
- [Getting Started](#getting-started)
  - [Backend](#running-the-backend)
  - [Frontend](#running-the-angular-client)
- [Test Credentials](#test-credentials)
- [Running Unit Tests](#running-unit-tests)
- [Known Limitations](#known-limitations)

## Technologies

**Backend**
- ASP.NET Core 10 (Web API + Razor Pages)
- Entity Framework Core (Code First, migrations, Fluent API)
- ASP.NET Core Identity (password hashing, roles)
- JWT Bearer authentication
- SQLite

**Frontend**
- Angular (standalone components, signals, functional guards/interceptors)
- HttpClient for API communication

**Testing**
- xUnit
- EF Core InMemory provider

## Architecture

The backend follows a simple, explainable layered flow:

```
Controller → Service → DbContext → Database
```

- **Controller** — accepts HTTP requests, delegates to a Service, returns the HTTP response. No business logic.
- **Service** — contains business logic (duplicate checks, ownership rules, ShortCode generation), works with `DbContext`.
- **DbContext** (`AppDbContext`) — EF Core's gateway to the database, configured via Fluent API.

DTOs (`ShortUrlDto`, `LoginRequest`, etc.) are used at the API boundary instead of exposing EF entities directly.

No Repository pattern, CQRS, MediatR, or Clean Architecture layering was added — the project is small enough that these would add indirection without a concrete benefit here.

### Why JWT instead of cookie-based auth

The app mixes an Angular SPA (talking to the API) with a classic Razor Page (`About`). JWT was chosen as the single auth mechanism: Angular stores the token in `localStorage` and sends it via an HTTP interceptor; the `About` Razor Page reads/writes the same token via a small inline script and calls the same protected API endpoints. This avoids running two different auth systems (cookie for Razor, JWT for API) side by side.

**Trade-off:** because Angular (`localhost:4200`) and the backend (`localhost:5019`) are different origins during development, `localStorage` is not shared between them. The `About` page therefore has its own minimal login form. See [Known Limitations](#known-limitations).

## Implemented Functionality

- Login (email + password) issuing a JWT
- Short URLs Table (Angular), visible to everyone
- Add new URL — visible only to authenticated users, no page reload
- Duplicate URL detection (`409 Conflict`), enforced at the database level via a `UNIQUE` index
- Delete URL — ownership-based for regular users, unrestricted for Admins, no page reload
- Short URL Info page — authenticated users only
- Short URL redirect endpoint (`GET /{shortCode}`) — public, returns `404` if not found
- About page (Razor Page) — public read, Admin-only edit, persisted in the database
- Server-side authorization on every protected endpoint (frontend hiding is UX only, never the actual security boundary)
- Unit tests covering ShortCode generation, duplicate handling, and delete ownership rules

## Roles & Permissions

| Action                     | Anonymous | User            | Admin |
|-----------------------------|:---------:|:---------------:|:-----:|
| View URL table               | ✅        | ✅               | ✅    |
| Redirect via short URL        | ✅        | ✅               | ✅    |
| View Short URL Info          | ❌        | ✅               | ✅    |
| Create URL                    | ❌        | ✅               | ✅    |
| Delete own URL                | ❌        | ✅               | ✅    |
| Delete another user's URL      | ❌        | ❌               | ✅    |
| Read About                    | ✅        | ✅               | ✅    |
| Edit About                    | ❌        | ❌               | ✅    |

All rules above are enforced **server-side** (`[Authorize]` / `[Authorize(Roles = "Admin")]` and explicit ownership checks in `ShortUrlService`). The Angular UI hides unavailable actions for UX only — a manually crafted API request from a regular user to delete another user's URL is rejected with `403 Forbidden`.

## URL Shortening Algorithm

1. The new `ShortUrl` record is inserted into the database **without** a `ShortCode` — this lets the database generate a unique auto-incrementing `Id`.
2. That `Id` is encoded into a **Base62** string (alphabet: `a-z`, `A-Z`, `0-9` — 62 characters), the same way a number is converted into hexadecimal, just with a larger alphabet.
3. The `ShortCode` is saved back to the same record.

Both steps are wrapped in a database transaction, so a partially-created record (with a `null` ShortCode) can never persist if something fails mid-way.

**Why this is collision-free by construction:** the `Id` is a database primary key, guaranteed unique. Since the `ShortCode` is a deterministic function of a unique `Id`, two different URLs can never produce the same `ShortCode` — there is no random generation and therefore no retry-on-collision logic needed.

A `UNIQUE` index is still placed on `ShortCode` in the database as a defensive safety net, not because collisions are expected.

## Database

- SQLite, chosen for zero setup — no server installation needed to run or review the project.
- EF Core Code First: the schema is fully defined by C# entities (`ShortUrl`, `AppUser`, `AboutContent`) and Fluent API configuration in `AppDbContext.OnModelCreating`.
- Schema is created and evolved exclusively through EF Core migrations — no manual SQL scripts.
- `OriginalUrl` and `ShortCode` both have `UNIQUE` indexes at the database level (not just checked in C# before insert).
- The `.db` file is **not** committed to the repository — it is generated locally by running migrations (see below).

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js + npm
- Angular CLI (`npm install -g @angular/cli`)

### Running the Backend

```bash
cd src/UrlShortener.Api
dotnet ef database update
dotnet run
```

The API will be available at `http://localhost:5019`. On first run, the database is created via migrations and seeded with test users and an initial About text.

Swagger UI: `http://localhost:5019/swagger`
About page: `http://localhost:5019/About`

### Running the Angular Client

In a separate terminal:

```bash
cd client
npm install
ng serve
```

The app will be available at `http://localhost:4200`.

> The backend must be running first — the Angular app calls `http://localhost:5019/api` directly (configured in `client/src/environments/environment.ts`).

## Test Credentials

Seeded automatically on first backend run:

| Role  | Email              | Password    |
|-------|---------------------|-------------|
| User  | `user@test.com`      | `User123!`  |
| Admin | `admin@test.com`     | `Admin123!` |

## Running Unit Tests

```bash
dotnet test tests/UrlShortener.Tests
```

Covers:
- Base62 ShortCode generation (uniqueness of encoding for different inputs, known values)
- Duplicate `OriginalUrl` handling
- Delete: owner can delete own URL
- Delete: regular user cannot delete another user's URL
- Delete: Admin can delete any URL
- Not-found handling for `GetById` / `Delete` / redirect lookup
- `CanDelete` flag correctness across Anonymous / owner / other user / Admin
- About content read/update persistence

## Known Limitations

- **HTTPS redirection is disabled** for local development convenience (`UseHttpsRedirection()` is commented out in `Program.cs`). A production deployment should re-enable it.
- **JWT signing key is stored in `appsettings.json`** for simplicity. In a real deployment this should live in a secret manager or environment variable.
- **Angular and the `About` Razor Page use separate login sessions** during local development, since they run on different origins (`localhost:4200` vs `localhost:5019`) and `localStorage` is not shared across origins. In a production setup where Angular is built and served by the same ASP.NET Core host, this would not be an issue.
- No QR codes, click analytics, custom aliases, or URL expiration — intentionally out of scope per the task requirements.
