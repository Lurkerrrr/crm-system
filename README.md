# CRM System

A lightweight Customer Relationship Management (CRM) desktop application for small and medium-sized organizations.

## Overview

CRM System enables organizations to manage customer relationships, track interactions, monitor client lifecycle status, and generate analytical reports through a Windows desktop interface. The application focuses on essential CRM functionality without the complexity of large commercial systems.

## Features

- **Client management** — create, read, update, delete clients with full validation
- **Contact history** — log notes, meetings, phone calls, and emails per client
- **Client lifecycle** — track status (New, Active, In Negotiation, Closed) with business rules preventing invalid transitions
- **Search and filtering** — case-insensitive search across name, company, email; status filter; column sorting
- **Reports and analytics** — total counts, pie chart of clients by status, bar chart of contacts by type, configurable date-range queries
- **Data export** — CSV (UTF-8 with BOM for Excel compatibility) and JSON (camelCase, Polish characters preserved)
- **Input validation** — regex-based field validation with inline error messages

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 12 |
| Runtime | .NET 8 (LTS) |
| UI Framework | WPF |
| Architecture Pattern | MVVM |
| MVVM Library | CommunityToolkit.Mvvm 8.2.2 |
| ORM | Entity Framework Core 8 |
| Database | SQLite |
| Charts | LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc5 |
| CSV | CsvHelper 33 |
| JSON | System.Text.Json (built-in) |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Configuration | Microsoft.Extensions.Configuration |
| Testing | xUnit, Moq, FluentAssertions, EF Core InMemory |

## Architecture

The project follows a layered architecture with strict separation of concerns:

```
CRMSystem.UI         Presentation layer (WPF + MVVM)
CRMSystem.Business   Business logic layer (services, validation, exceptions)
CRMSystem.Data       Data access layer (EF Core, repositories, migrations)
CRMSystem.Domain     Domain layer (entities, enums)
```

Dependency direction: `UI → Business → Data → Domain`. The Domain layer has no dependencies on any other layer.

### Design Patterns

- **Repository Pattern** — generic `IRepository<T>` base with entity-specific repositories for custom queries
- **MVVM** — strict separation between Views (XAML), ViewModels (logic), and Models (entities)
- **Dependency Injection** — constructor injection throughout, configured via `Microsoft.Extensions.DependencyInjection`
- **Service Layer** — business rules encapsulated in `IClientService`, `IContactService`, `IReportService`, `IExportService`
- **Navigation Service** — pages resolved via DI, parameterized navigation for detail views

## Project Structure

```
crm-system/
├── src/
│   ├── CRMSystem.Domain/        Entities (Client, Contact, User) and Enums
│   ├── CRMSystem.Data/          CrmDbContext, repositories, migrations, entity configurations
│   ├── CRMSystem.Business/      Services, validation, custom exceptions
│   └── CRMSystem.UI/            WPF application
│       ├── Views/               XAML views and dialog windows
│       │   └── Controls/        Reusable UI controls (BusyOverlay)
│       ├── ViewModels/          One ViewModel per View
│       ├── Services/            UI services (Navigation, Dialog)
│       └── Resources/           Styles dictionary, application icon
├── tests/
│   └── CRMSystem.Tests/         78 unit and integration tests
│       └── Services/            One test class per business service
├── CRMSystem.sln                Solution file
├── LICENSE
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

On first run, click **"Seed Sample Data"** in the Clients view to populate the database with example records.

## Database

The application uses SQLite. The database file is created automatically on first run via EF Core migrations. To apply migrations manually:

```bash
dotnet ef database update --project src/CRMSystem.Data --startup-project src/CRMSystem.UI
```

## Tests

The test suite covers the business layer (services, validation, business rules) and the report layer (using EF Core's InMemory provider).

```bash
dotnet test
```

Test categories:
- **Validation tests** — required fields, length limits, format rules
- **Business logic tests** — state machine rules (e.g., "closed clients cannot be reactivated directly")
- **Cross-entity tests** — referential integrity (e.g., contacts must reference an existing client)
- **Report tests** — counts, grouping, date-range queries
- **Export tests** — CSV/JSON format correctness, UTF-8 encoding, Polish character preservation

Total: 78 automated tests, executing in approximately 2 seconds.