# CRM System

A lightweight Customer Relationship Management (CRM) desktop application for small and medium-sized organizations.

## Overview

CRM System enables organizations to manage customer relationships, track interactions, monitor sales pipelines, and generate analytical reports through a desktop interface. The application focuses on essential CRM functionality without the complexity of large commercial systems.

## Features

- Client management (create, read, update, delete)
- Contact history tracking (notes, meetings, calls, emails)
- Client lifecycle status (New, Active, In Negotiation, Closed)
- Search and filtering
- Basic reporting and analytics
- Data export (CSV/JSON)
- Optional user authentication with role-based access

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# |
| Runtime | .NET 8 (LTS) |
| UI Framework | WPF |
| Architecture Pattern | MVVM |
| MVVM Library | CommunityToolkit.Mvvm |
| ORM | Entity Framework Core 8 |
| Database | SQLite |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Configuration | Microsoft.Extensions.Configuration |
| Testing | xUnit, Moq, FluentAssertions |

## Architecture

The project follows a layered architecture with strict separation of concerns:

```
CRMSystem.UI         Presentation layer (WPF + MVVM)
CRMSystem.Business   Business logic layer (services, validation)
CRMSystem.Data       Data access layer (EF Core, repositories)
CRMSystem.Domain     Domain layer (entities, enums)
```

Dependency direction: UI -> Business -> Data -> Domain. The Domain layer has no dependencies on any other layer.

### Design Patterns

- Repository Pattern
- MVVM (Model-View-ViewModel)
- Dependency Injection
- SOLID Principles

## Project Structure

```
crm-system/
├── src/
│   ├── CRMSystem.Domain/        Entities, enums
│   ├── CRMSystem.Data/          DbContext, repositories, migrations
│   ├── CRMSystem.Business/      Services, validation, business rules
│   └── CRMSystem.UI/            WPF application (Views, ViewModels)
├── tests/
│   └── CRMSystem.Tests/         Unit and integration tests
├── docs/                        Project documentation
├── CRMSystem.sln                Solution file
└── README.md
```

## Requirements

- .NET 8 SDK
- Windows OS (required for WPF)
- Visual Studio 2022 or later, or JetBrains Rider

## Build and Run

```bash
git clone https://github.com/Lurkerrrr/crm-system.git
cd crm-system

dotnet restore
dotnet build
dotnet run --project src/CRMSystem.UI
```

## Database

The application uses SQLite. The database file is created automatically on first run via EF Core migrations. To apply migrations manually:

```bash
dotnet ef database update --project src/CRMSystem.Data --startup-project src/CRMSystem.Data
```

## Tests

```bash
dotnet test
```

## Documentation

Full project documentation is available in the `docs/` folder.