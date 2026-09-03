namespace EcommerceApi.Api.Dtos;

public record CustomerDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
public record CreateCustomerDto(string FullName, string Email);