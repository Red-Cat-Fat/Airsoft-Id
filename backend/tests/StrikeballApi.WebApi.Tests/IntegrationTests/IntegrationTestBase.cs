using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Infrastructure.Data;
using Xunit;

namespace StrikeballApi.WebApi.Tests.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; } = null!;
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    private readonly string _dbName;

    protected IntegrationTestBase()
    {
        _dbName = $"test_db_{Guid.NewGuid()}";
    }

    public virtual async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "TestSecretKeyForTestingAtLeast32Characters!",
                    ["JwtSettings:Issuer"] = "TestIssuer",
                    ["JwtSettings:Audience"] = "TestAudience",
                    ["ConnectionStrings:DefaultConnection"] = "TestConnection"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ApplicationDbContext));
                if (contextDescriptor != null)
                    services.Remove(contextDescriptor);

                var interfaceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IApplicationDbContext));
                if (interfaceDescriptor != null)
                    services.Remove(interfaceDescriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                services.AddScoped<IApplicationDbContext>(sp =>
                    sp.GetRequiredService<ApplicationDbContext>());
            });
        });

        Client = Factory.CreateClient();
        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
