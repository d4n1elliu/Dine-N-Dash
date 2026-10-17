# Deployment (production VPS)

Three files make up the production stack:

| Path | Purpose |
|---|---|
| `server/DineNDash.Api/Dockerfile` | Multi-stage build of the .NET 10 API (build context `./server`) |
| `client/Dockerfile` + `client/nginx.conf` | Builds the React SPA and serves it with nginx (build context `./client`) |
| `docker-compose.prod.yml` | Wires both containers onto the external `proxy` network |
| `.github/workflows/deploy.yml` | GitHub Actions pipeline that deploys every push to `main` |

`docker-compose.yml` is still the local development stack (Postgres only) and is
not used in production.

## Automated deployment (GitHub Actions)

`.github/workflows/deploy.yml` deploys to the VPS on every push to `main` (and
on demand), independently of `.github/workflows/ci.yml`.

1. The runner writes `SSH_KEY` to a `chmod 600` file, then verifies the host key:
   `ssh-keyscan -t ed25519` fetches it and its SHA256 fingerprint must equal
   the pinned `SSH_HOST_FINGERPRINT` before it is added to `known_hosts` — no
   trust-on-first-use, and SSH still runs with `StrictHostKeyChecking=yes`.
2. Over SSH it runs, in `/var/www/dine-n-dash` (a pull-only clone):
   `git fetch --prune origin`, then `git reset --hard origin/main`, then
   `docker compose -f docker-compose.prod.yml --env-file .env.prod up --build -d`
   and `... ps`.
   `git reset --hard` only touches tracked files, so the untracked `.env.prod`
   survives; the script refuses to deploy if `.env.prod` is missing.
3. It then polls `https://dine-n-dash.sunwookim.dev/api/health` up to 10 times,
   10 seconds apart, and fails the job unless a 2xx comes back.

Required **repository secrets** (names only, set under
*Settings → Secrets and variables → Actions*):

| Secret | Purpose |
|---|---|
| `SSH_HOST` | VPS host name or IP |
| `SSH_USER` | SSH user that owns `/var/www/dine-n-dash` and can run Docker |
| `SSH_KEY` | Private key for that user (its public half in the server's `authorized_keys`) |
| `SSH_HOST_FINGERPRINT` | Pinned `SHA256:...` fingerprint of the server's ed25519 host key, taken from a trusted source — on the VPS console run `ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub` (or `ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key -E sha256`) and record the `SHA256:...` it prints. The deploy fails if the scanned key does not match, so a MITM cannot be trusted silently |

Never copy `SSH_HOST_FINGERPRINT` from `ssh-keyscan`: that sources the pin from the
same network path it protects, which is trust-on-first-use again. The workflow
only uses `ssh-keyscan` to compare the live key with this trusted value.

Run it manually from *Actions → Deploy → Run workflow*, or:

```bash
git push origin main                      # deploys automatically
gh workflow run deploy.yml --ref main      # manual trigger
gh run watch                               # follow the current run
```

Deploys are serialised by the `deploy-production` concurrency group and are
never cancelled midway. Application secrets (`POSTGRES_*`, `JWT_KEY`) are **not**
GitHub secrets — they live only in the server's untracked `.env.prod`.

## Manual deploy (fallback)

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

`Program.cs` calls `db.Database.MigrateAsync()` before the app starts listening,
in **every** environment, so the Production container applies any pending EF Core
migrations by itself on startup. A deploy against an empty database therefore
creates the schema, and a deploy with new migrations applies them — no manual
step, no separate migration container.

Migration failures abort startup (the container exits non-zero and the
`unless-stopped` restart policy will retry), which the pipeline surfaces as a
failed health check. Check the backend logs when that happens:

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod logs backend
```

## Useful commands

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod ps
docker compose -f docker-compose.prod.yml --env-file .env.prod logs -f backend
docker compose -f docker-compose.prod.yml --env-file .env.prod down        # keeps the uploads volume
docker compose -f docker-compose.prod.yml --env-file .env.prod up --build -d
```
