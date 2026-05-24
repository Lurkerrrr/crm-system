# Setup Guide

This guide walks you through getting the project from this initial commit onto GitHub and running on your machine.

## 1. Create the GitHub repository

1. Go to https://github.com/new
2. Repository name: **`crm-system`**
3. Description: *A lightweight CRM desktop application built with C#, WPF, and EF Core. University project at Rzeszów, 2026.*
4. Visibility: **Public**
5. **Do NOT** initialize with README, .gitignore, or LICENSE — we already have those.
6. Click **Create repository**.

## 2. Push this project to GitHub

Open a terminal in the `crm-system` folder and run:

```bash
git init
git add .
git commit -m "Initial commit: project structure, README, .NET solution scaffold"
git branch -M main
git remote add origin https://github.com/<YOUR_USERNAME>/crm-system.git
git push -u origin main
```

Replace `<YOUR_USERNAME>` with your GitHub username.

## 3. Open the solution

### Option A — Visual Studio 2022
1. Open Visual Studio 2022
2. File → Open → Project/Solution
3. Select `CRMSystem.sln`
4. Right-click the solution → **Restore NuGet Packages**
5. Build → Build Solution (Ctrl+Shift+B)

### Option B — JetBrains Rider
1. Open Rider
2. Open → select `CRMSystem.sln`
3. Rider auto-restores packages
4. Build → Build Solution (Ctrl+Shift+B)

### Option C — Command line
```bash
dotnet restore
dotnet build
```

## 4. Verify the structure

You should see this in your IDE:

```
CRMSystem.sln
├── src
│   ├── CRMSystem.Domain
│   ├── CRMSystem.Data
│   ├── CRMSystem.Business
│   └── CRMSystem.UI
└── tests
    └── CRMSystem.Tests
```

## 5. Set up branching

For solo work, I recommend a simple flow:

```bash
# Create a develop branch
git checkout -b develop
git push -u origin develop

# For each feature, branch from develop
git checkout -b feature/domain-entities
# ... do work ...
git add .
git commit -m "Add Client, Contact, User entities"
git push -u origin feature/domain-entities
# Open a Pull Request on GitHub → merge to develop
```

When everything is stable, merge `develop` → `main` for the demo-ready version.

## 6. Next steps

Once the repo is on GitHub and the solution builds cleanly, we'll move to **Phase 1**:

- Define `Client`, `Contact`, `User` entities
- Set up `CrmDbContext` with EF Core
- Create the first migration and database

Let me know when you've pushed to GitHub and we'll continue!
