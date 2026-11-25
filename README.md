# FinTrack

Portfolio & Transactions Tracker: an ASP.NET Core Web API with a Blazor WebAssembly frontend and a MySQL database.

## Live URLs

| Service | URL |
|---|---|
| Web app | https://fintrack.elitesgroup.co.ke/ |
| API (Swagger UI) | https://fintrackapi.elitesgroup.co.ke/swagger/index.html |

**Demo login:** `analyst@fintrack.co.ke` / `Demo@12345`

## Architecture

```
Browser ──► FinTrack.Web  (Blazor WASM, static files served by nginx)
   │
   └──────► FinTrack.Api  (ASP.NET Core 10, JWT auth, Swagger) ──► MySQL 8 (stored procedures)
```

| Path | Contents |
|---|---|
| `FinTrack.Api/` | REST API: auth, portfolios, transactions, holdings summary |
| `FinTrack.Web/` | Blazor WebAssembly client |
| `database/` | `001_schema.sql`, `002_procedures.sql`, `003_seed.sql` |
| `Dockerfile.backend` | Container image for the API |
| `Dockerfile.frontend` | Container image for the web app (nginx) |
| `docker/` | nginx config and the startup script that writes the web app's runtime config |

### API endpoints

| Method | Route | Auth |
|---|---|---|
| POST | `/api/auth/register` | – |
| POST | `/api/auth/login` | – |
| GET / POST | `/api/portfolios` | JWT |
| GET / PUT / DELETE | `/api/portfolios/{id}` | JWT |
| GET / POST | `/api/portfolios/{id}/transactions` | JWT |
| GET | `/api/portfolios/{id}/summary` | JWT |
| DELETE | `/api/transactions/{id}` | JWT |

To call a protected endpoint from Swagger, log in with `/api/auth/login`, click **Authorize**, and paste the returned token (without the word `Bearer`).

## Running locally

**Prerequisites:** .NET 10 SDK and MySQL 8.

1. **Create the database:**
   ```bash
   mysql -u root -p < database/001_schema.sql
   mysql -u root -p < database/002_procedures.sql
   mysql -u root -p < database/003_seed.sql   # optional demo data
   ```
2. **Configure the API.** Secrets are never committed: `appsettings.json` keeps them empty. Use user-secrets, or create `FinTrack.Api/appsettings.Development.json` (git-ignored); see `appsettings.Example.json` for the shape.
   ```bash
   cd FinTrack.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=fintrack_db;User ID=root;Password=<password>;"
   dotnet user-secrets set "Jwt:Key" "<random string, at least 32 characters>"
   ```
3. **Run the API** (http://localhost:5030/swagger):
   ```bash
   dotnet run --project FinTrack.Api
   ```
4. **Run the web app** (http://localhost:5032). It reads the API address from `FinTrack.Web/wwwroot/appsettings.json` (`ApiBaseUrl`).
   ```bash
   dotnet run --project FinTrack.Web
   ```

On startup the API seeds a demo analyst with three portfolios if `analyst@fintrack.co.ke` does not exist yet.

## Deployment (Coolify)

Both apps are deployed with [Coolify](https://coolify.io) from this GitHub repository, each built from its own Dockerfile. Coolify's proxy terminates HTTPS, and Cloudflare sits in front of both domains.

### 1. Database

1. In Coolify, create a **MySQL 8** database resource.
2. Run `database/001_schema.sql`, `002_procedures.sql` and (optionally) `003_seed.sql` against it.
3. Note the database's **internal hostname** (shown on the resource). The API uses it to connect over Coolify's internal Docker network.

### 2. API: `fintrackapi.elitesgroup.co.ke`

Create an **Application** from the repo with these settings:

| Setting | Value |
|---|---|
| Build pack | Dockerfile |
| Base directory | `/` |
| Dockerfile location | `/Dockerfile.backend` |
| Ports exposes | `8080` |
| Domain | `https://fintrackapi.elitesgroup.co.ke` |
| Connect to predefined network | Enabled (so the API can reach the database) |

Environment variables (mark each as **Is Literal**, untick **Build Variable**):

| Variable | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Server=<db-internal-host>;Port=3306;Database=fintrack_db;User ID=<user>;Password=<password>;Pooling=True;` |
| `Jwt__Key` | Random secret, **at least 32 bytes** (see below) |
| `Cors__AllowedOrigins__0` | `https://fintrack.elitesgroup.co.ke` |

Generate a JWT key (PowerShell):
```powershell
[Convert]::ToBase64String((1..48 | % { Get-Random -Max 256 }))
```

Swagger is enabled in production at `/swagger`. The container health check calls `/swagger/v1/swagger.json`.

### 3. Web app: `fintrack.elitesgroup.co.ke`

| Setting | Value |
|---|---|
| Build pack | Dockerfile |
| Base directory | `/` |
| Dockerfile location | `/Dockerfile.frontend` |
| Ports exposes | `80` |
| Domain | `https://fintrack.elitesgroup.co.ke` |

| Variable | Value |
|---|---|
| `API_BASE_URL` | `https://fintrackapi.elitesgroup.co.ke` |

The image is static, so the API address is not compiled in. On container start, `docker/40-fintrack-config.sh` writes `API_BASE_URL` into `appsettings.json`. To point at a different API, change the variable and restart; no rebuild is needed.

### 4. Deploy and verify

1. Push to `main`, then click **Deploy** on both applications (or enable auto-deploy).
2. Open https://fintrackapi.elitesgroup.co.ke/swagger/index.html and call `POST /api/auth/login` with the demo credentials. You should get a token back.
3. Open https://fintrack.elitesgroup.co.ke/ and sign in.

Environment variable changes only take effect after a **redeploy**.

### Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `500` on login/register, log shows `IDX10720 ... key size must be greater than 256 bits` | `Jwt__Key` is too short. Use at least 32 characters and redeploy. |
| API container exits on start with `... is not configured` | `ConnectionStrings__DefaultConnection` or `Jwt__Key` is missing or misspelled (note the double underscore). |
| `502` / "no available server" | **Ports exposes** doesn't match (`8080` for the API, `80` for the web app). |
| Login works in Swagger but the web app shows network/CORS errors | `Cors__AllowedOrigins__0` must exactly match the web URL (`https`, no trailing slash). |
| Browser console: `Unexpected token '<'` in `blazor.webassembly.js` | `index.html` must reference `_framework/blazor.webassembly#[.{fingerprint}].js` so that .NET 10 publish rewrites it to the fingerprinted file. |
| Mixed-content errors in the browser | `API_BASE_URL` must use `https://`. |
