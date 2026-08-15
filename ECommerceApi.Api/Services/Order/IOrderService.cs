using ECommerceApi.Api.Dtos;

namespace ECommerceApi.Api.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync(int? customerId);
    Task<OrderDto?> GetByIdAsync(int id);
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
}