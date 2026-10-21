using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ECommerce.Catalog.Tests;

/// <summary>
/// Runs the Catalog API against a throwaway PostgreSQL container (same image as the AppHost),
/// so constraints, unique indexes and migrations behave as in the real engine.
/// The container generates its own credentials: no secret lives in the tests.
/// </summary>
public class CatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Same key Aspire injects via WithReference(catalogDb). Migrations run at startup in Development.
        builder.UseSetting("ConnectionStrings:catalogdb", _postgres.GetConnectionString());
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
