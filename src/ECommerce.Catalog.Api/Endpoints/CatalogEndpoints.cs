using ECommerce.Catalog.Api.Data;
using ECommerce.Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Catalog.Api.Endpoints;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/categories", async (CatalogDbContext db) =>
            await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync())
            .WithTags("Catalog")
            .WithName("GetCategories");

        var group = routes.MapGroup("/api/products").WithTags("Catalog");

        group.MapGet("/", async (int? categoryId, string? search, CatalogDbContext db) =>
        {
            var query = db.Products.AsNoTracking();

            if (categoryId is not null)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Case-insensitive "contains"; LIKE wildcards typed by the user are matched literally.
                var pattern = $"%{EscapeLikePattern(search.Trim())}%";
                query = query.Where(p => EF.Functions.ILike(p.Name, pattern, @"\"));
            }

            return await query.OrderBy(p => p.Id).ToListAsync();
        })
        .WithName("GetProducts");

        group.MapGet("/{id:int}", async (int id, CatalogDbContext db) =>
            await db.Products.FindAsync(id) is { } product
                ? Results.Ok(product)
                : Results.NotFound())
            .WithName("GetProductById");

        group.MapPost("/", async (CreateProductRequest request, CatalogDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Sku) || request.Sku.Length > CatalogDbContext.SkuMaxLength)
            {
                return Results.BadRequest($"Sku is required and must be at most {CatalogDbContext.SkuMaxLength} characters.");
            }

            if (request.Price < 0 || request.AvailableStock < 0)
            {
                return Results.BadRequest("Price and AvailableStock must be zero or greater.");
            }

            if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            {
                return Results.BadRequest($"Category {request.CategoryId} does not exist.");
            }

            if (await db.Products.AnyAsync(p => p.Sku == request.Sku))
            {
                return Results.Conflict($"A product with Sku '{request.Sku}' already exists.");
            }

            var product = new Product
            {
                Sku = request.Sku,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                AvailableStock = request.AvailableStock,
                CategoryId = request.CategoryId
            };

            db.Products.Add(product);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Another request inserted the same Sku between the check above and this save.
                return Results.Conflict($"A product with Sku '{request.Sku}' already exists.");
            }

            return Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product);
        })
        .WithName("CreateProduct");

        return group;
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}

public record CreateProductRequest(string Sku, string Name, string? Description, decimal Price, int AvailableStock, int CategoryId);
