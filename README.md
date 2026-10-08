# Dine N Dash

An online restaurant searching and reservation service.

## Key Features

- Separate accounts for diners and restaurant owners
- Search restaurants by name, cuisine and location
- Filter and sort results
- View restaurant details, photos and menus
- Book a table and manage bookings
- Leave reviews and ratings
- Customer dashboard for bookings, reviews, bookmarks and profile details
- Restaurant owner dashboard for menus, photos and incoming bookings
- Input validation and clear error messages throughout

## Tech Stack

- Frontend: React, Tailwind CSS, TypeScript
- Backend: C# with ASP.NET Core Web API (.NET 10)
- Database: PostgreSQL with Entity Framework Core, running in Docker
- Testing: NUnit
- Deployment: VPS
- Containerization: Docker

## System Architecture

The React frontend sends HTTP requests to the ASP.NET Core Web API. The API passes each request to a service in the Core project, which uses a repository in the Data project to read and write to PostgreSQL through Entity Framework Core.

```text
React frontend  →  DineNDash.Api  →  DineNDash.Core  →  DineNDash.Data  →  PostgreSQL
```

The backend is split into separate projects so each layer has one responsibility and stays loosely coupled.

| Project | Responsibility |
|---|---|
| `DineNDash.Api` | Controllers, request and response models, error handling |
| `DineNDash.Core` | Models, enums, interfaces, services and business logic |
| `DineNDash.Data` | Entity Framework DbContext, repositories and migrations |
| `DineNDash.Tests` | NUnit tests for the services in Core |

`DineNDash.Core` has no dependency on the database or the API, which keeps the business logic easy to test.

## Project Structure

```text
Dine-N-Dash/
├── .env.example                         Example Docker database settings
├── .gitignore
├── CONTRIBUTING.md                      Team workflow and conventions
├── DineNDash.sln                        Solution file, open this in Visual Studio 2026
├── docker-compose.yml                   Runs PostgreSQL in Docker
├── README.md
└── apps/
    ├── web/                             React frontend
    │   ├── public/
    │   │   ├── favicon.svg              Browser tab icon
    │   │   └── icons.svg                Icon sprite
    │   ├── src/
    │   │   ├── App.tsx                  Root component
    │   │   ├── index.css                Tailwind import and global styles
    │   │   └── main.tsx                 App entry point
    │   ├── .gitignore
    │   ├── eslint.config.js             Linting rules
    │   ├── index.html                   HTML shell the app loads into
    │   ├── package-lock.json
    │   ├── package.json                 Dependencies and npm scripts
    │   ├── tsconfig.app.json            TypeScript settings for the app
    │   ├── tsconfig.json                Root TypeScript config
    │   ├── tsconfig.node.json           TypeScript settings for Vite config
    │   └── vite.config.ts               Vite and Tailwind setup
    │
    └── backend/                         C# backend
        ├── DineNDash.Api/               ASP.NET Core Web API
        │   ├── Controllers/             API endpoints
        │   ├── DTOs/                    Request and response models
        │   ├── Middleware/              Global error handling
        │   ├── Properties/
        │   │   └── launchSettings.json  Local ports and run settings
        │   ├── appsettings.json
        │   ├── DineNDash.Api.csproj
        │   └── Program.cs               App startup, services and CORS
        │
        ├── DineNDash.Core/              Business logic
        │   ├── Enums/                   Fixed values such as cuisine type and booking status
        │   ├── Extensions/              Extension methods
        │   ├── Interfaces/              Service and repository interfaces
        │   ├── Models/                  Restaurants, menu items, bookings, reviews and user accounts
        │   ├── Services/                Search, filtering and validation using LINQ
        │   └── DineNDash.Core.csproj
        │
        ├── DineNDash.Data/              Database layer
        │   ├── Migrations/              Entity Framework migrations
        │   ├── Repositories/            Generic and specific repositories
        │   ├── AppDbContext.cs          Entity Framework database context
        │   └── DineNDash.Data.csproj
        │
        └── DineNDash.Tests/             NUnit tests
            ├── Services/                Tests for the Core services
            └── DineNDash.Tests.csproj
```

## Prerequisites

- Visual Studio 2026 with the **ASP.NET and web development** workload
- .NET 10 SDK
- Node.js 22 LTS or later
- Docker Desktop

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/d4n1elliu/Dine-N-Dash.git
cd Dine-N-Dash
```

### 2. Start the database

Copy `.env.example` to `.env` in the repo root and set a database password. Then start PostgreSQL in Docker.

```bash
docker compose up -d
```

Store the connection string with .NET User Secrets so it never gets committed. Use the same password you set in `.env`.

```bash
cd apps/backend/DineNDash.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=dinendash;Username=postgres;Password=yourpassword"
```

Apply the database migrations.

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project ../DineNDash.Data --startup-project .
```

### 3. Run the backend

**In Visual Studio 2026**

1. Open `DineNDash.sln`
2. Right-click `DineNDash.Api` and select **Set as Startup Project**
3. Press **F5**

**From the terminal**

```bash
dotnet run --project apps/backend/DineNDash.Api
```

The API docs open at `http://localhost:5166/scalar`.

### 4. Run the frontend

Copy `apps/web/.env.example` to `apps/web/.env` and set `VITE_API_URL` to the URL the backend is running on.

```bash
cd apps/web
npm install
npm run dev
```

Open the link shown in the terminal, usually `http://localhost:5173`.

### Stopping the database

```bash
docker compose down
```

## Running Tests

**In Visual Studio 2026** open **Test → Test Explorer** and click **Run All Tests**.

**From the terminal**

```bash
dotnet test
```

## Environment Variables

Secrets are never stored in the repository.

| Where | What goes there |
|---|---|
| `.env` in the repo root | Database settings used by Docker |
| .NET User Secrets | Database connection string and any private API keys |
| `apps/web/.env` | Public frontend settings such as `VITE_API_URL` |

## Deployment

The app is deployed on a VPS. Only code merged into `main` is deployed.

## Team

| Name | Contribution | Role |
|---|---|---|
| Daniel Liu | | |
| Sun Woo Kim | | |
| Gurpreet Kaur | Bookings, Reviews, Customer Dashboard (bookmarks, edit profile details, view bookings) | Code Requirements |