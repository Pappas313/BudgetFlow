using System.Reflection;
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

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter a valid JWT access token. Example: \"Bearer eyJhbGciOi...\"",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securitySchemeReference = new OpenApiSecuritySchemeReference("Bearer");

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                { securitySchemeReference, new List<string>() },
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
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
