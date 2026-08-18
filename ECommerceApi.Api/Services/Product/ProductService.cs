using AutoMapper;
using ECommerceApi.Api.Data;
using ECommerceApi.Api.Dtos;
using ECommerceApi.Api.Models;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Api.Exceptions;

namespace ECommerceApi.Api.Services;

public class ProductService : IProductService
{
    private readonly ECommerceDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ECommerceDbContext db, IMapper mapper, ILogger<ProductService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var query = _db.Products.Include(p => p.Category).AsNoTracking().AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var products = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _db.Products.Include(p => p.Category)
            .AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            throw new NotFoundException($"Category {dto.CategoryId} does not exist.");

        var product = _mapper.Map<Product>(dto);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created product {ProductId}: {Name}", product.Id, product.Name);

        // Reload with Category included so the response DTO can flatten CategoryName
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return false;

        _mapper.Map(dto, product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated product {ProductId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted product {ProductId}", id);
        return true;
    }
}