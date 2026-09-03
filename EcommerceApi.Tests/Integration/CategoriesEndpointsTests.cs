using System.Net;
using System.Net.Http.Json;
using EcommerceApi.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace EcommerceApi.Tests.Integration;

public class CategoriesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoCategoriesExist()
    {
        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_ShouldReturn201_WithLocationHeader()
    {
        var dto = new CreateCategoryDto("Books", "Reading material");
        _client.AuthenticateAs(TestAuthHelper.GenerateAdminToken());

        var response = await _client.PostAsJsonAsync("/api/categories", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        created!.Name.Should().Be("Books");
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameIsEmpty()
    {
        var dto = new CreateCategoryDto("", null);
        _client.AuthenticateAs(TestAuthHelper.GenerateAdminToken());

        var response = await _client.PostAsJsonAsync("/api/categories", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenCategoryDoesNotExist()
    {
        var response = await _client.GetAsync("/api/categories/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturn401_WhenNoTokenProvided()
    {
        var dto = new CreateProductDto("Test Product", null, 10.00m, 5, 1);

        var response = await _client.PostAsJsonAsync("/api/products", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}