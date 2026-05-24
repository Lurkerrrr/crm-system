# CRM System

A lightweight Customer Relationship Management (CRM) desktop application designed for small and medium-sized organizations. Built as a university team project (*Projekt Zespołowy*) at the University of Rzeszów, 2026.

## 📋 Overview

CRM System enables organizations to manage customer relationships, track interactions, monitor sales pipelines, and generate analytical reports — all through an intuitive desktop interface. Unlike bloated commercial CRMs, this system focuses on the essentials: simplicity, speed, and ease of use.

## ✨ Features

- **Client Management** — add, edit, delete, and search client records
- **Contact History** — log notes, meetings, calls, and other interactions
- **Status Tracking** — track client lifecycle (New, Active, In Negotiation, Closed)
- **Search & Filter** — quickly find data across the database
- **Reporting** — basic analytics (client counts, interaction frequency, etc.)
- **Data Export** — export to CSV/JSON
- **User Authentication** — optional login with role-based access

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# |
| Runtime | .NET 8 (LTS) |
| UI Framework | WPF (Windows Presentation Foundation) |
| Architecture Pattern | MVVM |
| ORM | Entity Framework Core |
| Database | SQLite |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Testing | xUnit |

## 🏗️ Architecture

The project follows a **layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────┐
│   CRMSystem.UI (WPF + MVVM)         │  ← Presentation Layer
├─────────────────────────────────────┤
│   CRMSystem.Business (Services)     │  ← Business Logic Layer
├─────────────────────────────────────┤
│   CRMSystem.Data (EF Core, Repos)   │  ← Data Access Layer
├─────────────────────────────────────┤
│   CRMSystem.Domain (Entities)       │  ← Domain Layer
└─────────────────────────────────────┘
```

### Design Patterns Used

- **Repository Pattern** — abstracts data access logic
- **MVVM** — separates UI from business logic
- **Dependency Injection** — improves testability and modularity
- **SOLID Principles** — applied throughout

## 📁 Project Structure

```
crm-system/
├── src/
│   ├── CRMSystem.Domain/        # Entities, enums, domain logic
│   ├── CRMSystem.Data/          # DbContext, repositories, migrations
│   ├── CRMSystem.Business/      # Services, validation, business rules
│   └── CRMSystem.UI/            # WPF application (Views, ViewModels)
├── tests/
│   └── CRMSystem.Tests/         # Unit and integration tests
├── docs/                        # Project documentation
├── CRMSystem.sln                # Solution file
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows OS (required for WPF)
- Visual Studio 2022 or JetBrains Rider (recommended)

### Build & Run

```bash
# Clone the repository
git clone https://github.com/<your-username>/crm-system.git
cd crm-system

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the WPF application
dotnet run --project src/CRMSystem.UI
```

### Database Setup

The application uses SQLite, which requires no separate installation. The database file is created automatically on first run. To apply migrations manually:

```bash
dotnet ef database update --project src/CRMSystem.Data --startup-project src/CRMSystem.UI
```

## 🧪 Running Tests

```bash
dotnet test
```

## 📚 Documentation

Full project documentation (in Polish) is available in the [`docs/`](./docs) folder.

## 👤 Author

**Viktor Pylypenko** (68166) — Group 6IID-P

**Supervisor:** mgr inż. Aleksander Mysakowec

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](./LICENSE) file for details.

---

*University of Rzeszów, 2026*
