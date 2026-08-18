using AutoMapper;
using ECommerceApi.Api.Data;
using ECommerceApi.Api.Dtos;
using ECommerceApi.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ECommerceApi.Api.Exceptions;

namespace ECommerceApi.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ECommerceDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ECommerceDbContext db, IMapper mapper, ILogger<CategoryService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _db.Categories.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return false;

        _mapper.Map(dto, category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return false;

        _db.Categories.Remove(category);
        
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Failed to delete category {CategoryId} - likely still referenced by products", id);
            throw new ConflictException($"Cannot delete category {id} because it still has products assigned to it.");
        }

        _logger.LogInformation("Delete category {CategoryId}", id);
        return true;
    }
}