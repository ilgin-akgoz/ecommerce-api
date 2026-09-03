using EcommerceApi.Api.Dtos;

namespace EcommerceApi.Api.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<bool> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
}