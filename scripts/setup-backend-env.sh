#!/usr/bin/env bash
#
# Set up the local environment for the DineNDash backend.
#
# Reads the database settings from .env, creates .env with a random database
# password when missing, generates a JWT signing key, and saves the values the
# API needs to .NET User Secrets:
#   - ConnectionStrings:Default
#   - Jwt:Key
#
# Usage:
#   ./scripts/setup-backend-env.sh            # save to .NET User Secrets
#   eval "$(./scripts/setup-backend-env.sh --export)"   # export to this shell
#   ./scripts/setup-backend-env.sh --help
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"
ENV_EXAMPLE="$REPO_ROOT/.env.example"
API_PROJECT="$REPO_ROOT/server/DineNDash.Api"

usage() {
    cat <<'EOF'
Usage: scripts/setup-backend-env.sh [--export]

Creates .env with a random POSTGRES_PASSWORD when missing, generates JWT_KEY
when missing, and saves the connection string and JWT key as .NET User Secrets
for the API.

Options:
  --export   Print export statements for the current shell instead of saving
             to .NET User Secrets. Use with: eval "$(scripts/setup-backend-env.sh --export)"
  -h, --help Show this help message
EOF
}

log() {
    printf '%s\n' "$*" >&2
}

mode="secrets"
case "${1:-}" in
    "") ;;
    --export) mode="export" ;;
    -h | --help)
        usage
        exit 0
        ;;
    *)
        log "Unknown option: $1"
        usage >&2
        exit 2
        ;;
esac

# Load simple KEY=value lines from .env without executing the file.
load_env() {
    local line key value
    while IFS= read -r line || [[ -n "$line" ]]; do
        line="${line%$'\r'}"
        [[ "$line" =~ ^[[:space:]]*# ]] && continue
        [[ "$line" != *=* ]] && continue

        key="${line%%=*}"
        value="${line#*=}"
        key="${key#"${key%%[![:space:]]*}"}"
        key="${key%"${key##*[![:space:]]}"}"
        [[ "$key" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]] || continue

        if [[ "$value" =~ ^\"(.*)\"$ ]] || [[ "$value" =~ ^\'(.*)\'$ ]]; then
            value="${BASH_REMATCH[1]}"
        fi

        printf -v "$key" '%s' "$value"
        export "$key"
    done <"$1"
}

# Generate URL-safe random text so the values can be used in .env and a
# connection string without escaping.
random_hex() {
    local bytes="$1"
    if command -v openssl >/dev/null 2>&1; then
        openssl rand -hex "$bytes"
    else
        od -An -N"$bytes" -tx1 /dev/urandom | tr -d ' \n'
    fi
}

set_env_value() {
    local key="$1" value="$2" temp_file
    if grep -qE "^[[:space:]]*${key}=" "$ENV_FILE"; then
        temp_file="$(mktemp)"
        sed -E "s|^[[:space:]]*${key}=.*|${key}=${value}|" "$ENV_FILE" >"$temp_file"
        mv "$temp_file" "$ENV_FILE"
    else
        printf '\n%s=%s\n' "$key" "$value" >>"$ENV_FILE"
    fi
}

env_created=false
if [[ ! -f "$ENV_FILE" ]]; then
    if [[ ! -f "$ENV_EXAMPLE" ]]; then
        log "Missing $ENV_FILE and $ENV_EXAMPLE."
        exit 1
    fi
    cp "$ENV_EXAMPLE" "$ENV_FILE"
    env_created=true
    log "Created .env from .env.example."
fi

load_env "$ENV_FILE"

missing=()
for name in POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD; do
    if [[ -z "${!name:-}" ]]; then
        missing+=("$name")
    fi
done

if (( ${#missing[@]} > 0 )); then
    log "Missing ${missing[*]} in $ENV_FILE."
    log "Set them, then run this script again."
    exit 1
fi

if [[ "$env_created" == true && "$POSTGRES_PASSWORD" == "change-me" ]]; then
    POSTGRES_PASSWORD="$(random_hex 24)"
    set_env_value "POSTGRES_PASSWORD" "$POSTGRES_PASSWORD"
    export POSTGRES_PASSWORD
    log "Generated a random database password in .env."
fi

# Do not silently continue with the placeholder password. Changing the password
# in an existing .env would not update a database volume that already exists.
if [[ "$POSTGRES_PASSWORD" == "change-me" ]]; then
    log "POSTGRES_PASSWORD in .env is still 'change-me'."
    log "Set a unique password in .env, or delete .env and run this script again to generate one."
    exit 1
fi

# Reuse the key from .env so restarting the backend does not invalidate tokens.
jwt_key="${JWT_KEY:-}"
if (( ${#jwt_key} < 32 )); then
    jwt_key="$(random_hex 32)"
    set_env_value "JWT_KEY" "$jwt_key"
    log "Generated a new JWT signing key in .env."
fi

connection_string="Host=localhost;Port=5432;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD"

if [[ "$mode" == "export" ]]; then
    printf 'export ASPNETCORE_ENVIRONMENT=%q\n' "${ASPNETCORE_ENVIRONMENT:-Development}"
    printf 'export ConnectionStrings__Default=%q\n' "$connection_string"
    printf 'export Jwt__Key=%q\n' "$jwt_key"
    exit 0
fi

if ! command -v dotnet >/dev/null 2>&1; then
    log "dotnet was not found. Install the .NET 10 SDK and run this script again."
    exit 1
fi

set_secret() {
    local key="$1" value="$2" output
    if ! output="$(dotnet user-secrets set "$key" "$value" --project "$API_PROJECT" 2>&1)"; then
        log "Failed to save $key:"
        log "$output"
        exit 1
    fi
    log "Saved $key to .NET User Secrets."
}

set_secret "ConnectionStrings:Default" "$connection_string"
set_secret "Jwt:Key" "$jwt_key"

log "Backend environment is ready."
log "Run it with: dotnet run --project server/DineNDash.Api"
