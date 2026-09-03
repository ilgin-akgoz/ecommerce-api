using EcommerceApi.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceApi.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Key", "test-only-signing-key-not-used-anywhere-real-32chars+");
        builder.UseSetting("Jwt:Issuer", "EcommerceApi");
        builder.UseSetting("Jwt:Audience", "EcommerceApiClients");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ECommerceDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)) ||
                    d.ServiceType == typeof(ECommerceDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid()));
        });
    }
}