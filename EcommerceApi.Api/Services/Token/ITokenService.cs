using EcommerceApi.Api.Models;

namespace EcommerceApi.Api.Services;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}