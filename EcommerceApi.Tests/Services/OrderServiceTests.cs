using AutoMapper;
using ECommerceApi.Api.Data;
using ECommerceApi.Api.Dtos;
using ECommerceApi.Api.Exceptions;
using ECommerceApi.Api.Mapping;
using ECommerceApi.Api.Models;
using ECommerceApi.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerceApi.Tests.Services;

public class OrderServiceTests
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

    private static async Task<(ECommerceDbContext db, Customer customer, Product product)> SeedBasicData(int stock = 10)
    {
        var db = CreateInMemoryContext();
        var category = new Category { Name = "Electronics" };
        var product = new Product { Name = "Mouse", Price = 25.00m, StockQuantity = stock, Category = category };
        var customer = new Customer { FullName = "Jane Doe", Email = "jane@example.com" };

        db.Categories.Add(category);
        db.Products.Add(product);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        return (db, customer, product);
    }

    [Fact]
    public async Task CreateAsync_ShouldDecrementStock_WhenOrderSucceeds()
    {
        var (db, customer, product) = await SeedBasicData(stock: 10);
        var service = new OrderService(db, CreateMapper(), NullLogger<OrderService>.Instance);

        var dto = new CreateOrderDto(customer.Id, new List<CreateOrderItemDto> { new(product.Id, 3) });
        await service.CreateAsync(dto);

        var updatedProduct = await db.Products.FindAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(7);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenStockInsufficient()
    {
        var (db, customer, product) = await SeedBasicData(stock: 2);
        var service = new OrderService(db, CreateMapper(), NullLogger<OrderService>.Instance);

        var dto = new CreateOrderDto(customer.Id, new List<CreateOrderItemDto> { new(product.Id, 5) });
        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<BusinessRuleException>();

        var unchangedProduct = await db.Products.FindAsync(product.Id);
        unchangedProduct!.StockQuantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_ShouldSnapshotCurrentPrice_OnOrderItem()
    {
        var (db, customer, product) = await SeedBasicData();
        var service = new OrderService(db, CreateMapper(), NullLogger<OrderService>.Instance);

        var dto = new CreateOrderDto(customer.Id, new List<CreateOrderItemDto> { new(product.Id, 1) });
        var result = await service.CreateAsync(dto);

        result.Items.Single().UnitPrice.Should().Be(25.00m);

        product.Price = 999.00m;
        await db.SaveChangesAsync();

        var reloadedOrder = await service.GetByIdAsync(result.Id);
        reloadedOrder!.Items.Single().UnitPrice.Should().Be(25.00m);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        var db = CreateInMemoryContext();
        var service = new OrderService(db, CreateMapper(), NullLogger<OrderService>.Instance);

        var dto = new CreateOrderDto(999, new List<CreateOrderItemDto> { new(1, 1) });
        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}