namespace BudgetFlow.API.Extensions;

/// <summary>
/// Extension methods for configuring ASP.NET Core health checks.
/// </summary>
public static class HealthCheckServiceCollectionExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }
}
