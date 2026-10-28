namespace BudgetFlow.API.Contracts;

/// <summary>
/// Response payload returned by the health check endpoint.
/// </summary>
/// <param name="Status">The overall health status (e.g., "Healthy", "Degraded", "Unhealthy").</param>
/// <param name="Application">The name of the application reporting its health.</param>
public sealed record HealthCheckResponse(string Status, string Application);
