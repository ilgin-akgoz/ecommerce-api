using AutoMapper;
using EcommerceApi.Api.Data;
using EcommerceApi.Api.Dtos;
using EcommerceApi.Api.Exceptions;
using EcommerceApi.Api.Mapping;
using EcommerceApi.Api.Models;
using EcommerceApi.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EcommerceApi.Tests.Services;

public class ProductServiceTests
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

    private static async Task<Category> SeedCategory(ECommerceDbContext db, string name = "Electronics")
    {
        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var category = await SeedCategory(db);
        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);
        var dto = new CreateProductDto("Mouse", "Wireless mouse", 25.00m, 10, category.Id);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Name.Should().Be("Mouse");
        result.CategoryName.Should().Be("Electronics");
        (await db.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);
        var dto = new CreateProductDto("Mouse", "Wireless mouse", 25.00m, 10, CategoryId: 999);

        // Act
        var act = async () => await service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        (await db.Products.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        var db = CreateInMemoryContext();
        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WithCategoryNameFlattened()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var category = await SeedCategory(db, "Books");
        var product = new Product { Name = "Novel", Price = 15.00m, StockQuantity = 3, Category = category };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);

        // Act
        var result = await service.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Books");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCategoryId()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var electronics = await SeedCategory(db, "Electronics");
        var books = await SeedCategory(db, "Books");

        db.Products.Add(new Product { Name = "Mouse", Price = 25.00m, StockQuantity = 10, Category = electronics });
        db.Products.Add(new Product { Name = "Novel", Price = 15.00m, StockQuantity = 3, Category = books });
        await db.SaveChangesAsync();

        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);

        // Act
        var result = await service.GetAllAsync(categoryId: electronics.Id, minPrice: null, maxPrice: null);

        // Assert
        result.Should().ContainSingle();
        result.Single().Name.Should().Be("Mouse");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByPriceRange()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var category = await SeedCategory(db);

        db.Products.Add(new Product { Name = "Cheap Item", Price = 5.00m, StockQuantity = 10, Category = category });
        db.Products.Add(new Product { Name = "Mid Item", Price = 50.00m, StockQuantity = 10, Category = category });
        db.Products.Add(new Product { Name = "Expensive Item", Price = 500.00m, StockQuantity = 10, Category = category });
        await db.SaveChangesAsync();

        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);

        // Act
        var result = await service.GetAllAsync(categoryId: null, minPrice: 10m, maxPrice: 100m);

        // Assert
        result.Should().ContainSingle();
        result.Single().Name.Should().Be("Mid Item");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var db = CreateInMemoryContext();
        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);
        var dto = new UpdateProductDto("Updated Name", null, 30.00m, 5, 1);

        var result = await service.UpdateAsync(999, dto);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingProduct()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var category = await SeedCategory(db);
        var product = new Product { Name = "Mouse", Price = 25.00m, StockQuantity = 10, Category = category };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);
        var dto = new UpdateProductDto("Gaming Mouse", "RGB edition", 45.00m, 20, category.Id);

        // Act
        var result = await service.UpdateAsync(product.Id, dto);

        // Assert
        result.Should().BeTrue();
        var updated = await db.Products.FindAsync(product.Id);
        updated!.Name.Should().Be("Gaming Mouse");
        updated.Price.Should().Be(45.00m);
        updated.StockQuantity.Should().Be(20);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var db = CreateInMemoryContext();
        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct_WhenNotReferencedByAnyOrder()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var category = await SeedCategory(db);
        var product = new Product { Name = "Mouse", Price = 25.00m, StockQuantity = 10, Category = category };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var service = new ProductService(db, CreateMapper(), NullLogger<ProductService>.Instance);

        // Act
        var result = await service.DeleteAsync(product.Id);

        // Assert
        result.Should().BeTrue();
        (await db.Products.CountAsync()).Should().Be(0);
    }
}
