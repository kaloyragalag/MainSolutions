# MainSolutions

A full-stack solution with a .NET 10 Web API and React 19 (TypeScript) frontend.

## Structure

```
MainSolutions/
├── docker-compose.yml          ← SQL Server 2022 container
├── MainSolutions.sln           ← .NET solution
├── MainSolutions.API/          ← .NET 10 Web API + EF Core
│   ├── Controllers/
│   ├── Data/AppDbContext.cs
│   ├── Models/
│   ├── Program.cs
│   └── appsettings.json
└── MainSolutions.React/        ← React 19 + TypeScript
    ├── public/
    └── src/
        ├── App.tsx             ← Home screen
        └── App.css
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Getting Started

### 1. Start SQL Server

```bash
docker-compose up -d
```

### 2. Run the API

```bash
cd MainSolutions.API
dotnet restore
dotnet run
```

API + Swagger: **https://localhost:5001/swagger**

### 3. Run the React App

```bash
cd MainSolutions.React
npm install
npm start
```

React App: **http://localhost:3000**

## EF Core Migrations

```bash
# From solution root
dotnet ef migrations add InitialCreate --project MainSolutions.API
dotnet ef database update --project MainSolutions.API
```

## Notes

- CORS is pre-configured to allow `http://localhost:3000` → API
- Change `SA_PASSWORD` in `docker-compose.yml` and `appsettings.json` before deploying
