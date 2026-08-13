namespace ECommerceApi.Api.Dtos;

public record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    string CategoryName);

public record CreateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);

public record UpdateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);