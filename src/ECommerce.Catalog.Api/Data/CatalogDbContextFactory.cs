using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Catalog.Api.Data;

/// <summary>
/// Used by <c>dotnet ef</c> outside Aspire, where no connection string is injected.
/// <c>migrations add</c> and <c>migrations script</c> never open a connection, so no
/// password is needed here — never add one.
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var port = Environment.GetEnvironmentVariable("CATALOG_DB_PORT") ?? "5433";
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql($"Host=localhost;Port={port};Database=catalogdb;Username=postgres")
            .Options;

        return new CatalogDbContext(options);
    }
}
