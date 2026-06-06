# MainSolutions

A full-stack solution with a .NET 10 Web API, React 19 TypeScript frontend, and xUnit test project.

## Solution Structure

```
MainSolutions/
├── .vscode/
├── MainSolutions.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Repositories/
│   │   └── Interfaces/
│   └── Services/
│       └── Interfaces/
├── MainSolutions.React/
│   └── src/
│       ├── components/
│       │   └── Layout/
│       ├── context/
│       ├── pages/
│       ├── services/
│       └── types/
└── MainSolutions.Test/
    ├── Controllers/
    ├── Repositories/
    └── Services/
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

### 2. Apply database migrations

```bash
cd MainSolutions.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run the API

```bash
cd MainSolutions.API
dotnet run
```

API: **https://localhost:5001**
Swagger UI: **https://localhost:5001/swagger**

### 4. Run the React app

```bash
cd MainSolutions.React
npm install
npm start
```

React app: **http://localhost:3000**

### 5. Run in VS Code (both together)

Open the `MainSolutions` root folder in VS Code, go to **Run & Debug** (`Ctrl+Shift+D`), select **Launch API + React** and press `F5`.

## Seed Data

On first run the API automatically seeds an admin user:

| Field    | Value                        |
|----------|------------------------------|
| Email    | `admin@mainsolutions.com`    |
| Password | `Admin@123`                  |

## API Endpoints

| Method | Endpoint              | Description              | Auth required |
|--------|-----------------------|--------------------------|---------------|
| POST   | /api/auth/login       | Authenticate user        | No            |
| POST   | /api/auth/register    | Register new user        | No            |

## Running Tests

```bash
cd MainSolutions.Test
dotnet test
```

To see detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test coverage

| File                      | Tests | What is covered                                      |
|---------------------------|-------|------------------------------------------------------|
| AuthServiceTests.cs       | 6     | Login, wrong password, inactive user, register, duplicate email, password hashing |
| AuthControllerTests.cs    | 4     | 200 on login, 401 on bad credentials, 201 on register, 409 on duplicate |
| UserRepositoryTests.cs    | 8     | GetByEmail, GetById, Create, Update, Exists — with in-memory DB |

## Tech Stack

| Layer      | Technology                          |
|------------|-------------------------------------|
| API        | .NET 10, ASP.NET Core               |
| ORM        | Entity Framework Core 10            |
| Database   | SQL Server 2022 (Docker)            |
| Auth       | JWT Bearer + BCrypt                 |
| API Docs   | NSwag (Swagger UI)                  |
| Frontend   | React 19, TypeScript, React Router  |
| Testing    | xUnit, Moq, FluentAssertions        |

## Notes

- Change `SA_PASSWORD` in `docker-compose.yml` and `appsettings.json` before deploying
- Change `Jwt:Key` in `appsettings.json` to a strong secret before deploying
- CORS is pre-configured to allow `http://localhost:3000`