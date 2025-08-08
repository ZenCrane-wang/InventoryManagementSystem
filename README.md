# InventoryERP - Minimal .NET 8 + EF Core (MySQL) Starter

**What you received**

- Minimal two-project skeleton:
  - `InventoryERP.API` - ASP.NET Core Web API
  - `InventoryERP.Infrastructure` - EF Core DbContext, Entities, Repository & UnitOfWork

**How to run**

1. Ensure .NET 8 SDK is installed on the server.
2. Edit `InventoryERP.API/appsettings.json` if you need to change the connection string.
   The current connection string was set to the value you provided.
3. From the server shell:

```bash
cd /path/to/InventoryERP/InventoryERP.API
dotnet restore
dotnet run
```

4. API swagger UI will be available at `http://localhost:5000/swagger` (or check console for exact URL).

**Notes**

- This is a minimal demo skeleton. You should:
  - Add authentication (JWT).
  - Add proper validation, DTOs and AutoMapper.
  - Replace EnsureCreated with EF Core migrations for production.
  - Harden DB connection (limit allowed IPs, use TLS).
