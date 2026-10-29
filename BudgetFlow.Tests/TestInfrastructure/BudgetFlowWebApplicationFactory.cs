using System.Linq;
using BudgetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetFlow.Tests.TestInfrastructure;

/// <summary>
/// Spins up the BudgetFlow API in-memory for integration tests: replaces the SQL Server
/// DbContext registration with an EF Core InMemory provider (unique per factory instance) and
/// supplies the minimal configuration (JWT signing key) required for the API to start.
/// </summary>
internal sealed class BudgetFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=BudgetFlowTests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Jwt:Key"] = "integration-test-signing-key-at-least-32-bytes-long",
                ["Jwt:Issuer"] = "BudgetFlow.Tests",
                ["Jwt:Audience"] = "BudgetFlow.Tests",
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BudgetFlowDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<BudgetFlowDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
