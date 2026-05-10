# Noble Bank API Repository Wiki

## Project Overview
Noble Bank API is a layered ASP.NET Core Web API for digital banking scenarios.  
It currently provides core functionality for:
- User registration and login
- JWT-based authentication and authorization
- Card request and retrieval flows
- Loan request and retrieval flows

The project is **actively in development**. Functionality, endpoints, domain rules, and internal structure may change as new features are introduced and existing features are refined.

---

## Goals of the API
- Provide a secure backend API for banking-related features.
- Keep business rules in a dedicated application/domain flow.
- Use clear separation of concerns with clean project boundaries.
- Support testability across API, application, infrastructure, and domain layers.

---

## Current Architecture
The repository follows a layered structure:

### `NobleBank.API`
Presentation layer:
- HTTP controllers (`AuthController`, `CardsController`, `LoansController`)
- Request pipeline setup in `Program.cs`
- Swagger/OpenAPI exposure in development
- JWT auth configuration and CORS policy
- Global exception handling middleware (`ExceptionHandlingMiddleware`)

### `NobleBank.Application`
Application/business orchestration layer:
- CQRS-style commands and queries
- MediatR handlers and pipeline behavior
- FluentValidation command validation
- AutoMapper DTO mapping
- Application interfaces (`IApplicationDbContext`, `ITokenService`, `IIdentityService`)

### `NobleBank.Domain`
Core domain model:
- Entities: `Card`, `Loan`, `Transaction`, `Post`, `ApplicationUser`
- Domain enums and constants
- Domain exceptions/results
- Domain events (example: `CardBlockedEvent`)

### `NobleBank.Infrastructure`
Infrastructure and external concerns:
- Entity Framework Core persistence (`ApplicationDbContext`)
- SQL Server provider integration
- Identity implementation
- JWT token generation implementation
- AES encryption service for stored card numbers
- Dependency injection registrations and settings binding

### Test Projects
- `NobleBank.API.Tests`
- `NobleBank.Application.Tests`
- `NobleBank.Domain.Tests`
- `NobleBank.Infrastructure.Tests`

These provide unit-level and behavior-oriented coverage across layers.

---

## API Functionality (Current State)

## Authentication
Controller: `api/auth`
- `POST /api/auth/register`
  - Registers a user via ASP.NET Core Identity.
  - Returns JWT token when successful.
- `POST /api/auth/login`
  - Authenticates credentials.
  - Returns JWT token when successful.

Validation highlights:
- Email must be valid.
- Password rules require length, uppercase letter, number, and special character.

## Cards
Controller: `api/cards` (authorized)
- `GET /api/cards`
  - Returns cards for the authenticated user.
- `GET /api/cards/{id}`
  - Returns a single card by id for the authenticated user.
- `POST /api/cards/request`
  - Creates a card request for the authenticated user.

Business highlights:
- Card number generation uses cryptographically secure random digits.
- Check digit is calculated with Luhn algorithm.
- Type/brand are validated; credit cards require positive credit limit.
- Card number is encrypted at EF Core persistence layer.

## Loans
Controller: `api/loans` (authorized)
- `GET /api/loans`
  - Returns loans for the authenticated user.
- `GET /api/loans/{id}`
  - Returns a single loan by id for the authenticated user.
- `POST /api/loans/request`
  - Creates a loan in `Pending` state for the authenticated user.

Business highlights:
- Loan amount and term validations are enforced.
- Interest rate is selected based on loan type.
- Monthly payment is derived in the domain model.
- Approval is intentionally separate from initial request workflow.

---

## Data and Persistence
- ORM: Entity Framework Core 8
- Database provider: SQL Server
- Identity: ASP.NET Core Identity integrated into the same DbContext
- `ApplicationDbContext` sets audit timestamps (`CreatedAt`, `UpdatedAt`) on save
- Domain events are dispatched through MediatR after successful save

### Card Number Encryption
- `Card.CardNumber` is encrypted/decrypted via EF Core value conversion.
- Encryption implementation uses AES with configured key and IV.
- `Last4Digits` and masked card display are used for safe presentation.

---

## Security Model (Current)
- JWT bearer authentication for protected endpoints.
- Authorization required for card and loan endpoints.
- Password and lockout policies configured in Identity options.
- Global exception middleware returns structured `ProblemDetails` responses.
- Settings-based secrets:
  - `Jwt:Secret`
  - `Encryption:Key`
  - `Encryption:IV`

> Note: Ensure production deployments use strong secrets management (e.g., secret vaults/environment configuration) and avoid committing sensitive values.

---

## Tooling and Technologies Used

### Runtime and Language
- .NET 8
- C#
- ASP.NET Core Web API

### Core Libraries
- MediatR (request/handler pipeline)
- FluentValidation (input/command validation)
- AutoMapper (mapping)
- EF Core + SQL Server provider (data access)
- ASP.NET Core Identity (user management)
- JWT token libraries (`System.IdentityModel.Tokens.Jwt`, `Microsoft.IdentityModel.Tokens`)
- Swashbuckle (Swagger/OpenAPI)

### Testing Stack
- xUnit
- Microsoft.NET.Test.Sdk
- coverlet.collector
- EF Core InMemory (in selected tests)

### CI
GitHub Actions workflow: `.github/workflows/dotnet-tests.yml`
- `dotnet restore`
- `dotnet build --configuration Release`
- `dotnet test --configuration Release`

---

## Configuration
Main configuration file: `NobleBank.API/appsettings.json`

Key sections:
- `ConnectionStrings:DefaultConnection`
- `Jwt` (`Secret`, `Issuer`, `Audience`, `ExpiryMinutes`)
- `Encryption` (`Key`, `IV`)

The infrastructure layer validates required encryption/JWT settings at startup.

---

## Repository Structure (High-Level)
- `NobleBank.API/` — API host, controllers, middleware
- `NobleBank.Application/` — use cases, validators, mappings, interfaces
- `NobleBank.Domain/` — entities, business rules, domain abstractions
- `NobleBank.Infrastructure/` — persistence, identity, token/encryption services
- `*.Tests/` — test suites per layer

---

## How to Build and Test Locally
From repository root:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

---

## Current Limitations and In-Progress Areas
Given active development status, some areas are intentionally incomplete or expected to evolve:
- API surface will expand with additional banking workflows.
- Domain workflows (e.g., approvals, transactions lifecycle) may be deepened.
- Authorization and role/permission granularity may be extended.
- Operational hardening (monitoring, deployment setup, environment-specific concerns) may evolve.

Treat this wiki as a **living document** and update it as features are added, removed, or changed.

---

## Maintenance Notes
When updating this project, keep this wiki aligned with:
- Endpoint behavior changes
- Domain rule changes
- Security/authentication updates
- New dependencies/tooling
- Architectural refactors

Keeping documentation current reduces onboarding time and prevents drift between implementation and expectations.
