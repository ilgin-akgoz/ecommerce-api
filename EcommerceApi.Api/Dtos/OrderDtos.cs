using EcommerceApi.Api.Models;

namespace EcommerceApi.Api.Dtos;

public record OrderItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
public record OrderDto
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public OrderStatus Status { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}
public record CreateOrderItemDto(int ProductId, int Quantity);
public record CreateOrderDto(int CustomerId, List<CreateOrderItemDto> Items);
