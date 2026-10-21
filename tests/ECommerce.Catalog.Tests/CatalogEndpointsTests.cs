// Tests d'integration des endpoints du catalogue.
//
// On demarre l'application reelle en memoire (WebApplicationFactory) et on l'interroge
// par HTTP. On teste des COMPORTEMENTS OBSERVABLES, pas des details d'implementation :
// c'est ce qui rend ces tests utiles quand le code interne change.
//
// Pre-requis : `public partial class Program { }` a la fin de Program.cs, pour que la
// classe generee par les top-level statements soit accessible depuis ce projet.
using System.Net;
using System.Net.Http.Json;
using ECommerce.Catalog.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ECommerce.Catalog.Tests;

public class CatalogEndpointsTests : IClassFixture<CatalogApiFactory>
{
    private readonly CatalogApiFactory _factory;

    public CatalogEndpointsTests(CatalogApiFactory factory) => _factory = factory;

    // ---------------------------------------------------------------- chemin nominal

    [Fact]
    public async Task GetProducts_RetourneUneListe()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
    }

    // ---------------------------------------------------------------- cas d'erreur
    // Le cas qui compte vraiment : un id inexistant doit rendre 404, PAS 200 avec un
    // corps vide ni 500. C'est le contrat que le Gateway et Ordering.Api consomment.

    [Fact]
    public async Task GetProductById_IdInexistant_Retourne404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------------- cas limite
    // Un id non entier ne doit pas atteindre le handler : la contrainte de route
    // `{id:int}` doit le rejeter en 404, et surtout pas provoquer une exception.

    [Fact]
    public async Task GetProductById_IdNonEntier_NeProvoquePasDErreurServeur()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products/abc");

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    // ---------------------------------------------------------------- creation + relecture
    // On verifie le cycle complet : la creation rend 201 avec un Location exploitable,
    // et la ressource est reellement lisible a cette adresse.

    [Fact]
    public async Task CreateProduct_PuisLecture_RetourneLeProduitCree()
    {
        var client = _factory.CreateClient();
        var requete = new { Sku = "KB-AZ-100", Name = "Clavier", Description = "AZERTY", Price = 49.90m, AvailableStock = 12, CategoryId = 1 };

        var creation = await client.PostAsJsonAsync("/api/products", requete);

        Assert.Equal(HttpStatusCode.Created, creation.StatusCode);
        Assert.NotNull(creation.Headers.Location);

        var relecture = await client.GetAsync(creation.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, relecture.StatusCode);

        var produit = await relecture.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(produit);
        Assert.Equal("Clavier", produit!.Name);
        Assert.Equal(49.90m, produit.Price);
        Assert.Equal(12, produit.AvailableStock);
    }

    // ---------------------------------------------------------------- health check
    // Les probes Kubernetes (lab 08) dependent de cet endpoint : on le teste ici,
    // pas seulement une fois le cluster monte.

    [Fact]
    public async Task Health_RepondOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
