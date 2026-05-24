# CRMSystem.UI

The **Presentation Layer** — a WPF application using the MVVM pattern. Depends on all other layers.

## Contents (planned)

- `App.xaml` / `App.xaml.cs` — application entry, DI container setup
- `Views/`
  - `MainWindow.xaml`
  - `DashboardView.xaml`
  - `ClientListView.xaml`
  - `ClientDetailsView.xaml`
  - `ClientFormView.xaml`
  - `ReportsView.xaml`
- `ViewModels/`
  - `MainViewModel.cs`
  - `DashboardViewModel.cs`
  - `ClientListViewModel.cs`
  - `ClientDetailsViewModel.cs`
  - `ClientFormViewModel.cs`
  - `ReportsViewModel.cs`
- `Converters/` — XAML value converters
- `Resources/` — styles, themes, icons
- `appsettings.json` — DB connection string, etc.
