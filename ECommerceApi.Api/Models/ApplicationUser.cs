using Microsoft.AspNetCore.Identity;

namespace EcommerceApi.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}