using AutoMapper;
using ECommerceApi.Api.Data;
using ECommerceApi.Api.Dtos;
using ECommerceApi.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Api.Services;

public class OrderService : IOrderService
{
    private readonly ECommerceDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ECommerceDbContext db, IMapper mapper, ILogger<OrderService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(int? customerId)
    {
        var query = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        var orders = await query.ToListAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        if (dto.Items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one item.");

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new InvalidOperationException($"Customer {dto.CustomerId} does not exist.");

        var order = new Order { CustomerId = dto.CustomerId };
        _db.Orders.Add(order);

        foreach (var item in dto.Items)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId)
                ?? throw new InvalidOperationException($"Product {item.ProductId} does not exist.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, requested: {item.Quantity}.");
            
            product.StockQuantity -= item.Quantity;

            order.OrderItems.Add(new OrderItem 
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Created order {OrderId} for customer {CustomerId} with {ItemCount} item(s)", order.Id, order.CustomerId, order.OrderItems.Count);

        await _db.Entry(order).Reference(o => o.Customer).LoadAsync();
        foreach(var item in order.OrderItems)
            await _db.Entry(item).Reference(oi => oi.Product).LoadAsync();

        return _mapper.Map<OrderDto>(order);
    }
}