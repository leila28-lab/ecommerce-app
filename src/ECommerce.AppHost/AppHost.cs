var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server — fixed host port and a data volume so data survives restarts.
// The password is an Aspire-generated parameter stored in the AppHost user secrets.
var postgres = builder.AddPostgres("postgres")
    .WithImageTag("17-alpine")
    .WithHostPort(5433)
    .WithDataVolume();

var catalogDb = postgres.AddDatabase("catalogdb");

// Catalog microservice — owns the catalogdb database.
var catalog = builder.AddProject<Projects.ECommerce_Catalog_Api>("catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

// Ordering microservice — depends on Catalog (resolved via service discovery).
var ordering = builder.AddProject<Projects.ECommerce_Ordering_Api>("ordering")
    .WithReference(catalog)
    .WaitFor(catalog);

// API Gateway (YARP) — the single entry point for the services.
var gateway = builder.AddProject<Projects.ECommerce_Gateway>("gateway")
    .WithReference(catalog)
    .WithReference(ordering)
    .WaitFor(catalog)
    .WaitFor(ordering);

// Blazor web frontend — talks to the services through the gateway.
builder.AddProject<Projects.ECommerce_Web>("web")
    .WithReference(gateway)
    .WaitFor(gateway)
    .WithExternalHttpEndpoints();

builder.Build().Run();
