using BudgetFlow.Application.Abstractions;
using BudgetFlow.Infrastructure.Identity;
using BudgetFlow.Infrastructure.Persistence;
using BudgetFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetFlow.Infrastructure.DependencyInjection;

/// <summary>
/// Registers all services provided by the Infrastructure layer, including
/// persistence, identity, and external service integrations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration, used to resolve the "DefaultConnection" connection string.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("The 'DefaultConnection' connection string is not configured.");

        services.AddDbContext<BudgetFlowDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsAssembly(typeof(BudgetFlowDbContext).Assembly.FullName)));

        services.AddDataProtection();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<BudgetFlowDbContext>()
            .AddDefaultTokenProviders();

        // TODO: Register repository implementations once they are created.

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
