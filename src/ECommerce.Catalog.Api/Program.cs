using ECommerce.Catalog.Api.Data;
using ECommerce.Catalog.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire: telemetry, health checks, resilience, service discovery.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// PostgreSQL via Aspire: "catalogdb" is the database resource name declared in the AppHost.
// Adds DbContext pooling, retries, a database health check, logging and telemetry.
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

var app = builder.Build();

// Aspire default endpoints (/health, /alive).
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCatalogEndpoints();

// Apply migrations on startup in Development only: elsewhere, a migration is an explicit,
// reviewed step of the delivery pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
}

app.Run();

// Exposes the generated Program class to WebApplicationFactory in integration tests.
public partial class Program { }
