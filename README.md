# Dine N Dash

An online restaurant searching and reservation service 

## Key Features

- Search restaurants by name, cuisine and location
- Filter and sort results
- View restaurant details
- Make and manage reservations
- Input validation and clear error messages throughout

## Tech Stack

- Frontend: React, TailwindCSS, TypeScript, JavaScript
- Backend: C# with ASP.NET Core Web API (.NET 9)
- Database: PostgreSQL with Entity Framework Core
- Testing: NUnit
- Deployment: Azure
- Containerization: Docker

## System Architecture

The React frontend sends HTTP requests to the ASP.NET Core Web API. The API passes each request to a service in the Core project, which uses a repository in the Data project to read and write to PostgreSQL through Entity Framework Core.

```text
React frontend  →  Backend.Api  →  Backend.Core  →  Backend.Data  →  PostgreSQL
```

The backend is split into separate projects so each layer has one responsibility and stays loosely coupled.

| Project | Responsibility |
|---|---|
| `Backend.Api` | Controllers, request and response models, error handling |
| `Backend.Core` | Models, enums, interfaces, services and business logic |
| `Backend.Data` | Entity Framework DbContext, repositories and migrations |
| `Backend.Tests` | NUnit tests for the services in Core |

`Backend.Core` has no dependency on the database or the API, which keeps the business logic easy to test.

## Project Structure

```text
Dine-N-Dash/
├── .gitignore
├── Backend.sln                          Solution file, open this in Visual Studio 2022
├── README.md
└── apps/
    ├── web/                             React frontend
    │   ├── public/
    │   │   ├── favicon.svg              Browser tab icon
    │   │   └── icons.svg                Icon sprite
    │   ├── src/
    │   │   ├── App.css                  Component styles
    │   │   ├── App.tsx                  Root component
    │   │   ├── index.css                Tailwind import and global styles
    │   │   └── main.tsx                 App entry point
    │   ├── .gitignore
    │   ├── eslint.config.js             Linting rules
    │   ├── index.html                   HTML shell the app loads into
    │   ├── package-lock.json
    │   ├── package.json                 Dependencies and npm scripts
    │   ├── README.md
    │   ├── tsconfig.app.json            TypeScript settings for the app
    │   ├── tsconfig.json                Root TypeScript config
    │   ├── tsconfig.node.json           TypeScript settings for Vite config
    │   └── vite.config.ts               Vite and Tailwind setup
    │
    └── backend/                         C# backend
        ├── Backend.Api/                 ASP.NET Core Web API
        │   ├── Controllers/             API endpoints
        │   ├── DTOs/                    Request and response models
        │   ├── Middleware/              Global error handling
        │   ├── Properties/
        │   │   └── launchSettings.json  Local ports and run settings
        │   ├── appsettings.json
        │   ├── Backend.Api.csproj
        │   └── Program.cs               App startup, services and CORS
        │
        ├── Backend.Core/                Business logic
        │   ├── Enums/                   Fixed values such as cuisine type and reservation status
        │   ├── Extensions/              Extension methods
        │   ├── Interfaces/              Service and repository interfaces
        │   ├── Models/                  Restaurant, Reservation, User and the shared BaseEntity
        │   ├── Services/                Search, filtering and validation using LINQ
        │   └── Backend.Core.csproj
        │
        ├── Backend.Data/                Database layer
        │   ├── Migrations/              Entity Framework migrations
        │   ├── Repositories/            Generic and specific repositories
        │   ├── AppDbContext.cs          Entity Framework database context
        │   └── Backend.Data.csproj
        │
        └── Backend.Tests/               NUnit tests
            ├── Services/                Tests for the Core services
            └── Backend.Tests.csproj
```

## Prerequisites

- Visual Studio 2022 version 17.12 or later, with the **ASP.NET and web development** workload
- .NET 9 SDK
- Node.js 22 LTS or later
- PostgreSQL 16 or later

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/d4n1elliu/Dine-N-Dash.git
cd Dine-N-Dash
```

### 2. Set up the database

Create a PostgreSQL database called `dinendash`. Then store the connection string with .NET User Secrets so it never gets committed.

```bash
cd apps/backend/Backend.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=dinendash;Username=postgres;Password=yourpassword"
```

Apply the database migrations.

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project ../Backend.Data --startup-project .
```

### 3. Run the backend

**In Visual Studio 2022**

1. Open `Backend.sln`
2. Right-click `Backend.Api` and select **Set as Startup Project**
3. Press **F5**

**From the terminal**

```bash
dotnet run --project apps/backend/Backend.Api
```

### 4. Run the frontend

Copy `apps/web/.env.example` to `apps/web/.env` and set `VITE_API_URL` to the URL the backend is running on.

```bash
cd apps/web
npm install
npm run dev
```

Open the link shown in the terminal, usually `http://localhost:5173`.

## Running Tests

**In Visual Studio 2022** open **Test → Test Explorer** and click **Run All Tests**.

**From the terminal**

```bash
dotnet test
```

## Environment Variables

Secrets are never stored in the repository.

| Where | What goes there |
|---|---|
| .NET User Secrets | Database connection string and any private API keys |
| `apps/web/.env` | Public frontend settings such as `VITE_API_URL` |

## Team

| Name | Role | Contribution |
|---|---|---|
| Daniel Liu | | |
| Sun Woo Kim| | |
| Gurpreet Kaur| Bookings, Reviews, Customer Dashboard (bookmarks, edit profile details, view bookings) | Code Requirements |