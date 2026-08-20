using Microsoft.AspNetCore.Identity;

namespace ECommerceApi.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}