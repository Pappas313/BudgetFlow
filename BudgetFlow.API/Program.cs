using BudgetFlow.API.Extensions;
using BudgetFlow.API.Middleware;
using BudgetFlow.Application.DependencyInjection;
using BudgetFlow.Infrastructure.DependencyInjection;
using BudgetFlow.Infrastructure.Persistence.Seeding;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the application's logging provider.
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddReactClientCors(builder.Configuration);
builder.Services.AddApiHealthChecks();

// Centralized exception handling.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Application and Infrastructure layer service registration.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerDocumentation();

    await DevelopmentDataSeeder.SeedAsync(app.Services);
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors(BudgetFlow.API.Extensions.CorsServiceCollectionExtensions.ReactClientPolicy);

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
