namespace ECommerceApi.Api.Dtos;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}

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