# CRMSystem.Business

The **Business Logic layer** contains services that orchestrate domain operations, enforce business rules, and perform validation. Depends on `CRMSystem.Domain` and `CRMSystem.Data`.

## Contents (planned)

- `Services/`
  - `IClientService.cs` / `ClientService.cs`
  - `IContactService.cs` / `ContactService.cs`
  - `IUserService.cs` / `UserService.cs`
  - `IReportService.cs` / `ReportService.cs`
  - `IExportService.cs` / `ExportService.cs` (CSV/JSON)
- `Validation/` — input validators
- `Dtos/` — data transfer objects (if needed)
