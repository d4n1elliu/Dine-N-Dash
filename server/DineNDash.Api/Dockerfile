# syntax=docker/dockerfile:1

# ------------------------------------------------------------------------------
# Build stage
# The build context is ./server so the Api's ProjectReferences to
# ../DineNDash.Core and ../DineNDash.Data resolve exactly as they do locally.
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first: `dotnet restore` is then cached until the
# package references or project graph actually change.
COPY DineNDash.Api/DineNDash.Api.csproj DineNDash.Api/
COPY DineNDash.Core/DineNDash.Core.csproj DineNDash.Core/
COPY DineNDash.Data/DineNDash.Data.csproj DineNDash.Data/
RUN dotnet restore DineNDash.Api/DineNDash.Api.csproj

# Then the rest of the solution and a Release publish of the API.
COPY . .
RUN dotnet publish DineNDash.Api/DineNDash.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app/publish

# ------------------------------------------------------------------------------
# Runtime stage
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# The API formats booking/times with Australian time zones, which the slim
# Debian runtime image does not ship by default.
# Kestrel listens on plain HTTP inside the container; TLS is terminated by the
# reverse proxy on the shared `proxy` network.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=Australia/Sydney

RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Uploaded images are written under ContentRootPath + Storage:ImagesPath
# (uploads/images). The directory is created here so it exists even before the
# named volume is mounted, and is owned by the non-root `app` user.
RUN mkdir -p /app/uploads/images && chown -R $APP_UID:$APP_UID /app

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "DineNDash.Api.dll"]
