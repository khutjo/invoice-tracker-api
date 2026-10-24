# Identity service

Registers businesses and their users, logs users in and issues the tokens every other service trusts.
It owns `identity_db` and nothing else; it never reads another service's database.

## Structure

```
Identity/
├─ InvoiceTracker.Identity.Api/
│  ├─ Controllers/        LoginController (login, refresh, logout), RegisterController (register, verify-email)
│  ├─ Services/           LoginService, RegisterService: the use cases behind the controllers
│  ├─ Interfaces/         ILoginService, IRegisterService
│  ├─ Models/Request/     LogInRequest, RegisterUserRequest, RefreshTokenRequest, VerifyEmailRequest (DataAnnotations)
│  ├─ Models/Response/    LoginResponse, RefreshTokenResponse, RegisterUserResponse
│  ├─ Options/            PortalOptions
│  ├─ Exceptions/         LoginException, RegisterException, IdentityExceptionHandler (maps them to ProblemDetails)
│  └─ InvoiceTracker.Identity.Api.http   manual requests for Rider
└─ InvoiceTracker.Identity.Infrastructure/
   ├─ Persistence/        IdentityServiceDbContext, ApplicationUser, UserRole, RefreshToken, Configurations/, Migrations/
   ├─ Tokens/             JwtOptions, SigningKeyProvider, AccessTokenIssuer, RefreshTokenService
   ├─ Interfaces/         IAccessTokenIssuer, IRefreshTokenService
   └─ DependencyInjection.cs   AddIdentityInfrastructure()
```

Identity is deliberately two projects: its rules are ASP.NET Core Identity's (hashing, lockout, password policy),
so a separate Domain and Application layer would add files without adding rules.

## Endpoints

All under `/api/v1/auth`, anonymous. Errors are RFC 9457 ProblemDetails.
In Development the OpenAPI document is at `/openapi/v1.json` and the Scalar UI at `/scalar`.

| Method | Path | Body | Success | Errors |
| --- | --- | --- | --- | --- |
| POST | `/register` | businessName, firstName, lastName, email, password | 201 `RegisterUserResponse` | 400 with `errors.email` / `errors.password` |
| POST | `/login` | email, password | 200 `LoginResponse` | 400, 401, 403 (email not verified) |
| POST | `/refresh` | refreshToken | 200 `RefreshTokenResponse` | 400, 401 |
| POST | `/logout` | refreshToken | 204 | 400 |
| POST | `/verify-email` | userId, token (from the email link) | 204 | 400 |

Health: `/health/live` (process is up) and `/health/ready` (`identity_db` answers).

Login returns the same 401 for an unknown email, a wrong password, an inactive user and a locked account,
so the API never reveals which emails are registered.

## Registration and events

Registration runs in one database transaction, so a user is never saved without its events:

1. Create the Owner with a new `BusinessId` (ASP.NET Core Identity hashes the password).
2. Generate an email confirmation token (bound to the user's security stamp, valid 24 hours).
3. Write two rows to `outbox_messages`:
   - `BusinessRegistered`: Business creates the business profile from it.
   - `EmailVerificationRequested`: Notification sends the link `{Portal:BaseUrl}{Portal:VerifyEmailPath}?userId=…&token=…`.
     The portal screen posts `userId` and `token` to `POST /api/v1/auth/verify-email`.
4. Commit.

`OutboxPublisherService` (BuildingBlocks.Messaging) publishes unprocessed rows every 2 seconds to the
`invoice-tracker.events` topic exchange, with the event type name as the routing key. It uses publisher confirms
and mandatory routing, so a row is marked processed only once RabbitMQ has stored the message in a queue.
If RabbitMQ is down, registration still succeeds and the events go out when it is back.

| Queue | Bound to |
| --- | --- |
| `business.events` | `BusinessRegistered` |
| `notification.events` | `EmailVerificationRequested` |

Delivery is at least once. The RabbitMQ message id is the event's `EventId`, which consumers use to drop duplicates.

A new user is registered but unverified (`users.email_confirmed = 0`). Login returns 403 until the link is used;
the 403 comes only after a correct password, so it never reveals whether an email is registered.

## Tokens

**Access token**: JWT signed with RSA SHA-256 (`kid` = `Jwt:KeyId`), 15 minutes.

| Claim | Value |
| --- | --- |
| `sub` | user id |
| `email` | user email |
| `business_id` | tenant id, the only source of tenant identity for every service |
| `role` | `Owner` or `Accountant` |
| `jti` | unique token id |

Claim names shared with other services live in `BuildingBlocks.ServiceDefaults/Authentication/InvoiceTrackerClaimTypes`.

Other services validate access tokens locally with Identity's RSA **public** key, supplied as configuration
(`Jwt:PublicKey`, from user-secrets locally and `Jwt__PublicKey` in `.env` for Docker). They never call Identity
to check a token. There is deliberately no JWKS endpoint yet: with one key and four services, a configured
public key is simpler and has no runtime dependency on Identity. Key rotation is where JWKS earns its place (see Planned).

**Refresh token**: 32 random bytes, base64url, 14 days, stored only as a SHA-256 hash in `refresh_tokens`.

- Each refresh revokes the presented token and issues a new one in the same family (rotation).
- Presenting a revoked token again revokes the whole family: a copied token ends the session for both holders.
- The revoke is a conditional `UPDATE ... WHERE revoked_at IS NULL`, so two concurrent refreshes with one token cannot both succeed.
- Logout revokes the family. It always returns 204.

**Lockout**: 5 failed passwords lock the account for 15 minutes. Refresh does not check lockout, so failed
logins against someone's email cannot end their existing session.

## Configuration

| Key | Where | Example |
| --- | --- | --- |
| `ConnectionStrings:IdentityDb` | user-secrets / `ConnectionStrings__IdentityDb` | SQL Server connection string |
| `Jwt:SigningKey` | user-secrets / `Jwt__SigningKey` (never in appsettings) | base64 PKCS#8 RSA 2048 private key |
| `Jwt:Issuer` | appsettings.json | `invoice-tracker-identity` |
| `Jwt:Audience` | appsettings.json | `invoice-tracker-api` |
| `Jwt:AccessTokenMinutes` | appsettings.json | `15` |
| `Jwt:RefreshTokenDays` | appsettings.json | `14` |
| `Jwt:KeyId` | appsettings.json | `identity-2026-10` |
| `RabbitMq:HostName`, `Port`, `VirtualHost`, `UserName`, `ClientName` | appsettings.json | `localhost`, `5672`, `/`, `invoice-tracker`, `identity` |
| `RabbitMq:Password` | user-secrets / `RabbitMq__Password` | same as `RABBITMQ_PASSWORD` in `.env` |
| `Portal:BaseUrl` | appsettings.json / `Portal__BaseUrl` | `http://localhost:8080` |
| `Portal:VerifyEmailPath` | appsettings.json | `/verify-email` (portal screen that calls `/verify-email`) |

The app fails at startup if any `Jwt`, `RabbitMq` or `Portal` value is missing.

## Database (`identity_db`)

| Table | Purpose |
| --- | --- |
| `users` | ASP.NET Core Identity users plus `business_id`, `role`, names, `is_active`, audit columns |
| `refresh_tokens` | `refresh_token_id`, `user_id`, `family_id`, `token_hash`, `created_at`, `expires_at`, `revoked_at` |
| `data_protection_keys` | Data Protection key ring that signs email verification tokens, so links survive restarts |
| `outbox_messages` | `outbox_message_id` (= EventId), `type`, `payload`, `occurred_at`, `processed_at`, `attempts`, `error` |
| `user_claims`, `user_logins`, `user_tokens` | Identity's own tables, unused for now |

## Tests

`tests/InvoiceTracker.Identity.IntegrationTests` runs the real API in memory (`WebApplicationFactory`) against a
throwaway SQL Server started by Testcontainers, so Docker must be running. The factory:

- uses the `Testing` environment, so developer user-secrets are never loaded;
- generates a fresh RSA key per run, so no secret is needed locally or in CI;
- applies the migrations, and removes the outbox publisher so events stay in `outbox_messages`, where tests assert on them without RabbitMQ.

Covered: registration and both outbox events, duplicate email and weak password field errors, 403 before
verification, tampered verification token, login after verification, identical 401s for every login failure,
lockout after five wrong passwords, refresh token rotation, reuse of a rotated token ending the session, and logout.

```powershell
dotnet test tests/InvoiceTracker.Identity.IntegrationTests
```

CI (`.github/workflows/ci.yml`) runs these on every push and pull request to `main`, alongside the format check
and a warnings-as-errors build.

