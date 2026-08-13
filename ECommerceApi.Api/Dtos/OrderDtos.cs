using ECommerceApi.Api.Models;

namespace ECommerceApi.Api.Dtos;

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public record OrderDto(
    int Id,
    DateTime CreatedAt,
    OrderStatus Status,
    int CustomerId,
    string CustomerName,
    decimal TotalAmount,
    List<OrderItemDto> Items);
public record CreateOrderItemDto(int ProductId, int Quantity);
public record CreateOrderDto(int CustomerId, List<CreateOrderItemDto> Items);
