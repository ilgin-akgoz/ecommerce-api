using AutoMapper;
using ECommerceApi.Api.Data;
using ECommerceApi.Api.Dtos;
using ECommerceApi.Api.Exceptions;
using ECommerceApi.Api.Mapping;
using ECommerceApi.Api.Models;
using ECommerceApi.Api.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace EcommerceApi.Tests.Services;

public class CategoryServiceTests
{
    private static ECommerceDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ECommerceDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCategoryToDatabase()
    {
        // arrange
        var db = CreateInMemoryContext();
        var service = new CategoryService(db, CreateMapper(), NullLogger<CategoryService>.Instance);
        var dto = new CreateCategoryDto("Electronics", "Gadgets and devices");

        // act
        var result = await service.CreateAsync(dto);

        // assert
        result.Name.Should().Be("Electronics");
        (await db.Categories.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        var db = CreateInMemoryContext();
        var service = new CategoryService(db, CreateMapper(), NullLogger<CategoryService>.Instance);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowConflictException_WhenCategoryHasProducts()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlite(connection)
            .Options;

        // seed with one context, then dispose it (simulating a separate request)
        await using (var seedDb = new ECommerceDbContext(options))
        {
            await seedDb.Database.EnsureCreatedAsync();

            var category = new Category { Name = "Electronics" };
            seedDb.Categories.Add(category);
            seedDb.Products.Add(new Product { Name = "Laptop", Price = 999, StockQuantity = 5, Category = category});
            await seedDb.SaveChangesAsync();
        }

        // fresh context, nothing pre-loaded in memory
        await using var db = new ECommerceDbContext(options);
        var categoryToDelete = await db.Categories.FirstAsync(c => c.Name == "Electronics");

        var service = new CategoryService(db, CreateMapper(), NullLogger<CategoryService>.Instance);
        var act = async() => await service.DeleteAsync(categoryToDelete.Id);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        var db = CreateInMemoryContext();
        var service = new CategoryService(db, CreateMapper(), NullLogger<CategoryService>.Instance);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}