# ProlabWeb

This is an ASP.NET Core MVC project using .NET 8, Individual Identity authentication, and PostgreSQL support.

## Features
- ASP.NET Core MVC
- Individual Identity authentication
- PostgreSQL database support (Npgsql)
- Ready for integration testing

## Getting Started

1. Update your `appsettings.json` with your PostgreSQL connection string.
2. Run database migrations:
   ```powershell
   dotnet ef database update --project .\ProlabWeb
   ```
3. Start the application:
   ```powershell
   dotnet run --project .\ProlabWeb
   ```

## Integration Testing
- Add your integration tests in the `ProlabWeb.Tests` project (to be created).
- Use a separate test database for integration tests.

---

For more details, see the official ASP.NET Core and Npgsql documentation.
