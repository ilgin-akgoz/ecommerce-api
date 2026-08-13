namespace ECommerceApi.Api.Dtos;

public record CustomerDto(int Id, string FullName, string Email);
public record CreateCustomerDto(string FullName, string Email);