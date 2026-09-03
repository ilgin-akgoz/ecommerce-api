namespace EcommerceApi.Api.Dtos;

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
public record CreateCategoryDto(string Name, string? Description);
public record UpdateCategoryDto(string Name, string? Description);