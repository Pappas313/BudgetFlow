using Microsoft.OpenApi;

namespace BudgetFlow.API.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation generation and UI.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BudgetFlow API",
                Version = "v1",
                Description = "API for the BudgetFlow personal budgeting application.",
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "BudgetFlow API v1");
        });

        return app;
    }
}
