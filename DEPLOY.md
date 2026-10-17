# Deployment (production VPS)

Three files make up the production stack:

| Path | Purpose |
|---|---|
| `server/DineNDash.Api/Dockerfile` | Multi-stage build of the .NET 10 API (build context `./server`) |
| `client/Dockerfile` + `client/nginx.conf` | Builds the React SPA and serves it with nginx (build context `./client`) |
| `docker-compose.prod.yml` | Wires both containers onto the external `proxy` network |

`docker-compose.yml` is still the local development stack (Postgres only) and is
not used in production.

## Deploy

```bash
# once, if the shared network does not exist yet
docker network create proxy

# every deploy
git pull
cp .env.prod.example .env.prod   # first time only, then edit the values
docker compose -f docker-compose.prod.yml --env-file .env.prod up --build -d
```

`--env-file .env.prod` is **mandatory**. Compose only auto-loads a file called
`.env`; without `--env-file` every `${...}` placeholder in
`docker-compose.prod.yml` interpolates to an empty string, which silently
produces a broken connection string such as `Host=postgres;Port=5432;Database=;Username=;Password=`
and the API fails to reach the database.

`.env.prod` is untracked — `.gitignore` ignores `.env.*`. Never commit it.

## Reverse proxy layout

Both containers publish no host ports; they are reachable only on the `proxy`
network. The host reverse proxy must map:

| Host | Path | Upstream |
|---|---|---|
| `dine-n-dash.sunwookim.dev` | `/api/*` | `http://dinendash-backend:8080` |
| `dine-n-dash.sunwookim.dev` | everything else | `http://dinendash-web:80` |

The SPA is built without `VITE_API_URL`, so it calls relative `/api/...` URLs and
works from any host name. The proxy must forward the `Host` header and set
`X-Forwarded-Proto: https` / `X-Forwarded-For` (the backend runs with
`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`); it terminates TLS itself.

## Backend environment variables

| Variable | Meaning |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` — disables Swagger/Scalar endpoints and dev exception pages |
| `ASPNETCORE_URLS` | `http://+:8080`, the port the container listens on (plain HTTP; TLS ends at the proxy) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Trusts `X-Forwarded-*` from the proxy so scheme/client IP and `UseHttpsRedirection()` are correct |
| `TZ` | `Australia/Sydney`, so the API's Australian time handling matches the host |
| `Frontend__Url` | The single CORS-allowed origin, `https://dine-n-dash.sunwookim.dev` |
| `ConnectionStrings__Default` | Npgsql connection string, interpolated from `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` in `.env.prod`; host is the shared `postgres` on the `proxy` network |
| `Jwt__Key` | Symmetric JWT signing key (`Jwt:Key` in config, fed from `JWT_KEY`). Must be at least 32 characters — there is no built-in fallback key, the API throws on startup if it is missing or too short |

Uploaded images are stored under `ContentRootPath/uploads/images` in the
container, so the named volume `dinendash-uploads` is mounted at `/app/uploads`
to keep them across redeploys. Containers run as the image's non-root `app` user
(UID 1654).

## Database migrations

`Program.cs` calls `db.Database.MigrateAsync()` on startup, but currently only
when `ASPNETCORE_ENVIRONMENT` is `Development` (see `Program.cs`, the
`if (app.Environment.IsDevelopment())` block around the migrate call). Because
this stack sets `Production`, **migrations do not run automatically here**.

Bootstrap the schema once against a fresh database by starting the already-built
image in Development mode, and stop it (`Ctrl-C`) as soon as you see
`Now listening on: http://[::]:8080`:

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod \
  run --rm -e ASPNETCORE_ENVIRONMENT=Development backend
```

Recommended follow-up: move the `MigrateAsync()` call out of the
`IsDevelopment()` guard in `Program.cs` so the Production container migrates
itself on start.

## Useful commands

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod ps
docker compose -f docker-compose.prod.yml --env-file .env.prod logs -f backend
docker compose -f docker-compose.prod.yml --env-file .env.prod down        # keeps the uploads volume
docker compose -f docker-compose.prod.yml --env-file .env.prod up --build -d
```
