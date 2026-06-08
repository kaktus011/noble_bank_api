# Noble Bank API — Repository Wiki

## Project Overview

Noble Bank API is a layered ASP.NET Core 8 Web API for digital banking scenarios.  
It provides a full backend for user authentication, card and loan management, transactions, posts, and administrative workflows.

The project follows **Clean Architecture** principles with strict separation between API, Application, Domain, and Infrastructure layers.

---

## Solution Structure

Eight projects in total:

| Project | Role |
|---|---|
| `NobleBank.API` | HTTP host — controllers, middleware, program setup |
| `NobleBank.Application` | CQRS use cases, validators, DTOs, interfaces |
| `NobleBank.Domain` | Entities, business rules, enums, domain exceptions |
| `NobleBank.Infrastructure` | EF Core, Identity, JWT, encryption, seeding |
| `NobleBank.API.Tests` | API-layer tests |
| `NobleBank.Application.Tests` | Application-layer tests |
| `NobleBank.Domain.Tests` | Domain-layer tests |
| `NobleBank.Infrastructure.Tests` | Infrastructure-layer tests |

---

## API Endpoints

### Authentication — `api/auth` (public)

| Method | Endpoint | Body | Response | Notes |
|---|---|---|---|---|
| POST | `/register` | `{ email, password, firstName, lastName }` | `{ token }` | Creates user with `User` role |
| POST | `/login` | `{ email, password, forceLogin? }` | `{ token }` or error | Rejects if active session exists unless `forceLogin: true` |
| POST | `/logout` | `{ token? }` or `Authorization` header | `200 OK` | Clears server-side session |

**Register validation:**
- Email: required, valid format
- Password: 8+ chars, 1 uppercase, 1 digit, 1 special character
- FirstName / LastName: required, max 50 chars each

---

### Cards — `api/cards` (requires auth)

| Method | Endpoint | Params / Body | Response | Notes |
|---|---|---|---|---|
| GET | `/` | — | `List<CardDto>` | Returns authenticated user's cards only |
| GET | `/{id:guid}` | — | `CardDto` or 404 | User-scoped by ID |
| POST | `/request` | `{ type, brand, creditLimit? }` | `CardDto` (201) | Creates card in `Pending` state |

**RequestCard validation:**
- `type`: valid `CardEnum.Type`
- `brand`: valid `CardEnum.Brand`
- `creditLimit`: required and > 0 for Credit cards; must be null for Debit / Virtual

---

### Loans — `api/loans` (requires auth)

| Method | Endpoint | Params / Body | Response | Notes |
|---|---|---|---|---|
| GET | `/` | — | `List<LoanDto>` | Returns authenticated user's loans only |
| GET | `/{id:guid}` | — | `LoanDto` or 404 | User-scoped by ID |
| POST | `/request` | `{ amount, termMonths, type }` | `LoanDto` (201) | Creates loan in `Pending` state |

**RequestLoan validation:**
- `amount`: 0.01 – 100,000
- `termMonths`: 1 – 360
- `type`: valid `LoansEnum.Type`

---

### Transactions — `api/transactions` (requires auth)

| Method | Endpoint | Params / Body | Response | Notes |
|---|---|---|---|---|
| GET | `/` | `?cardId=&limit=` (optional, default limit 50) | `List<TransactionDto>` | User-scoped, ordered by newest first |
| GET | `/{id:guid}` | — | `TransactionDto` or 404 | User-scoped by ID |
| POST | `/` | `{ cardId, amount, description, type }` | `TransactionDto` (201) | Records a transaction against a card |

**CreateTransaction validation:**
- `cardId`: required
- `amount`: > 0
- `description`: required, max 250 chars
- `type`: valid `TransactionsEnum.Type`

---

### Posts — `api/posts` (requires auth)

| Method | Endpoint | Auth | Body | Response | Notes |
|---|---|---|---|---|---|
| GET | `/` | User | — | `List<PostDto>` | All posts |
| GET | `/{id:guid}` | User | — | `PostDto` or 404 | Single post |
| POST | `/` | Administrator | `{ title, body }` | `PostDto` (201) | Admin only |
| DELETE | `/{id:guid}` | Administrator | — | 204 | Admin only |

**CreatePost validation:**
- `title`: required, max 200 chars
- `body`: required, max 5000 chars

---

### Admin — `api/admin` (requires `Administrator` role)

#### Cards
| Method | Endpoint | Body | Response | Notes |
|---|---|---|---|---|
| GET | `/cards` | — | `List<CardDto>` | All cards across all users |
| GET | `/cards/{id:guid}` | — | `CardDto` or 404 | Any card by ID |
| GET | `/cards/pending` | — | `List<CardDto>` | All pending card requests |
| POST | `/cards/{id:guid}/approve` | — | 204 | Activates card |
| POST | `/cards/{id:guid}/reject` | `{ reason }` | 204 | Rejects with reason |

#### Loans
| Method | Endpoint | Body | Response | Notes |
|---|---|---|---|---|
| GET | `/loans` | — | `List<LoanDto>` | All loans across all users |
| GET | `/loans/{id:guid}` | — | `LoanDto` or 404 | Any loan by ID |
| GET | `/loans/pending` | — | `List<LoanDto>` | All pending loan requests |
| POST | `/loans/{id:guid}/approve` | — | 204 | Activates loan |
| POST | `/loans/{id:guid}/reject` | `{ reason }` | 204 | Rejects with reason |

#### Transactions
| Method | Endpoint | Response | Notes |
|---|---|---|---|
| GET | `/transactions` | `List<TransactionDto>` | All transactions across all users (max 50) |
| GET | `/transactions/{id:guid}` | `TransactionDto` or 404 | Any transaction by ID |

---

## Domain Model

### BaseEntity
All domain entities extend `BaseEntity`:
- `Id: Guid`
- `CreatedAt: DateTime`
- `UpdatedAt: DateTime`
- `DomainEvents: IReadOnlyCollection<INotification>`

---

### ApplicationUser *(extends IdentityUser)*
| Property | Type | Notes |
|---|---|---|
| FirstName | string | |
| LastName | string | |
| FullName | string (computed) | FirstName + " " + LastName |
| SessionId | Guid? | Tracks active login session; null = no active session |
| Cards | ICollection\<Card\> | Navigation |
| Loans | ICollection\<Loan\> | Navigation |
| Posts | ICollection\<Post\> | Navigation |

---

### Card
| Property | Type | Notes |
|---|---|---|
| CardNumber | string | AES-encrypted at persistence layer |
| Last4Digits | string | Safe for display |
| CardHolder | string | Stored uppercase |
| Type | CardEnum.Type | Debit / Credit / Virtual |
| Brand | CardEnum.Brand | Visa / Mastercard / AmericanExpress / Maestro |
| Status | CardEnum.Status | Pending → Active / Rejected / Blocked / Inactive |
| Balance | decimal | |
| CreditLimit | decimal? | Credit cards only |
| Currency | string | Default: `EUR` |
| ExpiryDate | DateTime | 4 years from creation |
| IsExpired | bool (computed) | |
| MaskedNumber | string (computed) | `**** **** **** {last4}` |
| RejectionReason | string? | Set on rejection |
| UserId | string | FK to ApplicationUser |

**Card enums:**
- `CardEnum.Brand`: Visa (1), Mastercard (2), AmericanExpress (3), Maestro (4)
- `CardEnum.Type`: Debit (0), Credit (1), Virtual (2)
- `CardEnum.Status`: Unknown (0), Pending (1), Active (2), Inactive (3), Blocked (4), Rejected (5)

**Domain methods:**
- `Create(...)` — generates 16-digit Luhn-valid card number, sets status to Pending
- `Activate()` — Pending → Active
- `Reject(reason)` — Pending → Rejected
- `Block()` — Active → Blocked, publishes `CardBlockedEvent`
- `Deposit(amount)` — increases balance
- `Withdraw(amount)` — decreases balance; validates Active status, expiry, sufficient funds; returns `Result<decimal>`

---

### Loan
| Property | Type | Notes |
|---|---|---|
| Amount | decimal | 0.01 – 100,000 |
| RemainingAmount | decimal | Decreases with payments |
| InterestRate | decimal | Annual %, e.g. 4.5 |
| TermMonths | int | 1 – 360 |
| MonthlyPayment | decimal | Calculated with standard amortisation formula |
| Status | LoansEnum.Status | Pending → Active / Rejected / Closed |
| Type | LoansEnum.Type | Personal / Mortgage / Auto / Student |
| StartDate | DateTime | |
| EndDate | DateTime? | Set on approval or auto-close |
| RejectionReason | string? | |
| UserId | string | FK to ApplicationUser |

**Loan enums:**
- `LoansEnum.Status`: Active (0), Pending (1), Closed (2), Rejected (3)
- `LoansEnum.Type`: Personal (0), Mortgage (1), Auto (2), Student (3)

**Domain methods:**
- `Create(...)` — calculates monthly payment, sets status to Pending
- `Approve()` — Pending → Active, sets StartDate and EndDate
- `Reject(reason, performedBy)` — Pending → Rejected
- `MakePayment(amount, performedBy)` — reduces RemainingAmount; auto-closes (status → Closed) when fully paid; returns `Result<decimal>`

---

### Transaction
| Property | Type | Notes |
|---|---|---|
| Amount | decimal | Must be > 0 |
| Description | string | Max 250 chars |
| Type | TransactionsEnum.Type | Income / Expense / Transfer |
| OccurredAt | DateTime | |
| CardId | Guid | FK to Card |
| PerformedBy | string | User ID of performer |

**Transaction enum:**
- `TransactionsEnum.Type`: Income (0), Expense (1), Transfer (2)

---

### Post
| Property | Type | Notes |
|---|---|---|
| Title | string | Max 200 chars |
| Body | string | Max 5000 chars |
| UserId | string | FK to ApplicationUser (creator) |

---

## DTOs

### CardDto
`Id, Brand, Last4Digits, CardHolder, Type, Status, Balance, CreditLimit, Currency, ExpiryDate, IsExpired, IsCredit`

### LoanDto
`Id, Amount, RemainingAmount, InterestRate, TermMonths, MonthlyPayment, Status, Type, StartDate, EndDate, ProgressPercentage, RejectionReason, UserId`

> `ProgressPercentage` is computed: `(Amount - RemainingAmount) / Amount * 100`

### TransactionDto
`Id, Amount, Description, Type, OccurredAt, CardId, CardLast4`

### PostDto
`Id, Title, Body, CreatedAt, UpdatedAt`

---

## Authentication & Session Management

### JWT Tokens
- Algorithm: HS256 (symmetric)
- Default expiry: 60 minutes (configurable via `Jwt:ExpiryMinutes`)
- Claims: `sub` (UserId), `email`, `name`, `jti`, role claim (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`)

### Session Tracking
Each user has a `SessionId` (Guid?) stored in the database:
- **Login**: sets `SessionId` to a new Guid
- **Concurrent login**: returns an error if `SessionId` is already set — pass `forceLogin: true` in the request body to override
- **Logout**: clears `SessionId`; accepts token via `Authorization` header or request body (supports `sendBeacon` from browsers)

### Roles
| Role | Assigned By | Access |
|---|---|---|
| `User` | Auto-assigned on registration | Standard user endpoints |
| `Administrator` | Database seeder or manual assignment | All endpoints including `api/admin/*` |

---

## Infrastructure

### Encryption
- Card numbers are encrypted using **AES-256** via EF Core value conversion
- `IEncryptionService` → `AesEncryptionService`
- Key and IV must be provided as Base64-encoded strings via configuration

### Identity Configuration
- Password: 8+ chars, 1 uppercase, 1 digit, 1 special character
- Account lockout: 5 failed attempts → 15-minute lockout
- Unique email required per user

### Database
- ORM: Entity Framework Core 8
- Provider: SQL Server (`Server=(localdb)\mssqllocaldb` for local dev)
- `ApplicationDbContext` sets `CreatedAt` and `UpdatedAt` automatically on save
- Domain events dispatched via MediatR after each successful save

### MediatR Pipeline
1. `ValidationBehaviour` — runs FluentValidation validators before reaching handlers
2. Handler executes the command/query
3. Domain events dispatched post-save

---

## Exception Handling

`ExceptionHandlingMiddleware` catches all unhandled exceptions and returns structured **RFC 7807 ProblemDetails** responses:

| Exception | HTTP Status | Title |
|---|---|---|
| `NotFoundException` | 404 | Not Found |
| `DomainException` | 400 | Business Rule Violation |
| `ValidationException` | 400 | Validation Failed |
| `UnauthorizedAccessException` | 401 | Unauthorized |
| Anything else | 500 | An error occurred |

Validation errors include a field-level `errors` dictionary in the response body.

---

## Database Seeding

`DatabaseSeeder` runs automatically on startup when `ASPNETCORE_ENVIRONMENT = Development` (or when `RunDatabaseSeeding: true` is set in config).

**Always runs:**
- Creates `Administrator` and `User` roles if they don't exist
- Removes duplicate roles if found

**Conditional (requires `AdminSeeder` config):**
- Creates an admin user with the configured email and password
- Assigns the `Administrator` role
- If the user already exists, ensures they have the `Administrator` role

**Default config** (`appsettings.json`): `AdminSeeder.Disabled: true` — no admin user is seeded unless explicitly configured.

---

## Configuration

Main file: `NobleBank.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NobleBankDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "",
    "Issuer": "NobleBankApi",
    "Audience": "NobleBankClient",
    "ExpiryMinutes": 60
  },
  "Encryption": {
    "Key": "",
    "IV": ""
  },
  "AdminSeeder": {
    "Disabled": true,
    "Email": "",
    "Password": ""
  }
}
```

> `Jwt:Secret`, `Encryption:Key`, and `Encryption:IV` **must** be provided at runtime (via .NET user-secrets locally, environment variables, or a secret vault in production). The application validates these on startup and will fail to start if they are empty. **Never** put real values in `appsettings*.json` — those files are committed.
>
> `Encryption:Key` must be a Base64-encoded 32-byte value (AES-256).  
> `Encryption:IV` must be a Base64-encoded 16-byte value.  
> `Jwt:Secret` must be at least 32 characters.

---

## Security Notes

- **Never commit** `Jwt:Secret`, `Encryption:Key`, `Encryption:IV`, or `AdminSeeder:Password` to source control. `appsettings.Development.json` **is** tracked by git — do not put real secrets there. Use .NET user-secrets locally (`dotnet user-secrets set "Jwt:Secret" "..."`) or environment variables.
- **Production deployments** must use a secret vault (e.g., Azure Key Vault) or environment-level configuration injected by the deploy pipeline.
- If a secret is ever committed by accident, rotate it immediately — the git history makes it permanently exposed even after the file is scrubbed.
- Card numbers are always stored encrypted. `Last4Digits` is used for all display purposes.
- The 401 interceptor on the frontend wipes the local token and redirects to login on any 401 response.

---

## CORS

Single policy `ReactApp` configured for local development:
- Allowed origin: `https://localhost:5173` (configurable via `Cors:AllowedOrigins`)
- Allowed headers: any
- Allowed methods: any

---

## How to Build and Run Locally

```bash
# Restore and build
dotnet restore
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run the API (Development profile — uses https://localhost:7109)
cd NobleBank.API
dotnet run --launch-profile https
```

Before running for the first time, populate the secrets via **.NET user-secrets** (stored outside the repo, per-developer). From `NobleBank.API/`:

```bash
dotnet user-secrets set "Jwt:Secret"          "<32+ char random string>"
dotnet user-secrets set "Encryption:Key"      "<base64 of 32 random bytes>"
dotnet user-secrets set "Encryption:IV"       "<base64 of 16 random bytes>"
dotnet user-secrets set "AdminSeeder:Password" "<strong password>"
```

Generate the random values with a CSPRNG, e.g. in PowerShell:

```powershell
$b = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
[Convert]::ToBase64String($b)
```

Do **not** put these values in `appsettings.Development.json` — that file is committed to the repo.

Apply migrations:

```bash
dotnet ef database update --project NobleBank.Infrastructure --startup-project NobleBank.API
```

---

## CI

GitHub Actions workflow: `.github/workflows/dotnet-tests.yml`

Steps:
1. `dotnet restore`
2. `dotnet build --configuration Release`
3. `dotnet test --configuration Release`

---

## Maintenance Notes

Keep this wiki aligned with the codebase when making changes to:
- Endpoint paths, request/response shapes, or auth requirements
- Domain entity properties or business rules
- New validators or changes to existing validation logic
- Roles or permission model changes
- Infrastructure dependencies or settings
- Seeding behaviour
