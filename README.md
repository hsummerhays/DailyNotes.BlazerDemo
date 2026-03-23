# DailyNotes Blazor Demo

A modern, full-stack personal management application built with **.NET 10.0**, featuring a Blazor frontend, a robust ASP.NET Core API, and seamless Azure integration.

## 🌍 Live Application
- **Frontend (Blazor)**: [https://app-dn-blazor-hsh8.azurewebsites.net/](https://app-dn-blazor-hsh8.azurewebsites.net/)
- **Backend (API)**: [https://app-dn-api-hsh8.azurewebsites.net/](https://app-dn-api-hsh8.azurewebsites.net/)

## 🚀 Project Architecture

The solution follows a clean, decoupled architecture:

- **DailyNotes.Blazor**: An Interactive Server-side Blazor application providing the user interface, utilizing Microsoft Identity for authentication.
- **DailyNotes.Api**: A high-performance RESTful API managing data persistence and business logic.
- **DailyNotes.Shared**: A common library containing shared data models and constants.
- **DailyNotes.Tests**: A suite of xUnit tests for verifying configuration and core logic.

## 🛠️ Technology Stack

- **Frontend**: Blazor (Interactive Server Mode), Vanilla CSS, JavaScript.
- **Backend**: ASP.NET Core API, Entity Framework Core.
- **Identity**: Microsoft Identity Web (Azure AD / Microsoft Entra ID).
- **Database**: SQL Server (Production) / SQLite (Local Development).
- **Deployment**: Bicep (Infrastructure as Code), GitHub Actions (CI/CD).
- **Runtime**: .NET 10.0 Core.

## 💻 Local Development

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (v17.12+) or VS Code with C# Dev Kit.

### Setup
1. Clone the repository.
2. Initialize the local database:
   ```powershell
   dotnet ef database update --project DailyNotes.Api
   ```
3. Configure Azure AD (optional for local):
   - By default, the app uses a **Mock Authentication** mode in `Development` environments if no Client ID is provided.
4. Run the solution:
   ```powershell
   dotnet run --project DailyNotes.Blazor
   ```

## ☁️ Azure Deployment

This project is fully automated for Azure deployment using a Bicep-based infrastructure.

### Infrastructure Components
- **App Service Plan**: Basic (B1) Linux/Windows plan.
- **Web Apps**: Two separate instances for the API and Blazor frontend.
- **SQL Database**: Azure SQL (Basic tier) with automated firewall rules.

### Provisioning
Deploy the infrastructure using the Azure CLI:
```powershell
az deployment group create --resource-group rg-dailynotes-demo --template-file deploy.bicep --parameters sqlAdminPassword="<your-password>" azureAdClientSecret="<your-secret>"
```

### GitHub Actions (CI/CD)
The project includes a `.github/workflows/dotnet.yml` workflow that automatically:
1. Rebuilds and tests the application on every push to `main`.
2. Publishes and deploys code artifacts to Azure using **Publish Profiles**.
3. Utilizes `WEBSITE_RUN_FROM_PACKAGE=1` for immutable and reliable execution.

## 🔍 Health & Diagnostics
- **Health Endpoints**: Available at `/health` on both the API and Blazor apps.
- **Diagnostics Page**: A specialized production diagnostic tool is available at `/diagnostics` within the Blazor application to verify runtime configuration.

---
*Created and maintained as part of the DailyNotes showcase project.*
