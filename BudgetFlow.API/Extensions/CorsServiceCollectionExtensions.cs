namespace BudgetFlow.API.Extensions;

/// <summary>
/// Extension methods for configuring Cross-Origin Resource Sharing (CORS)
/// so the React frontend can call the API during development and production.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    public const string ReactClientPolicy = "ReactClient";

    public static IServiceCollection AddReactClientCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:5173", "http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(ReactClientPolicy, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
