using ECommerceApi.Api.Models;

namespace ECommerceApi.Api.Services;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}