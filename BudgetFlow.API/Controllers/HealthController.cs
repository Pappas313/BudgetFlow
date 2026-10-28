using BudgetFlow.API.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Exposes a lightweight health check endpoint for the BudgetFlow API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a simple status payload indicating the API is running.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResponse> Get(CancellationToken cancellationToken)
    {
        var response = new HealthCheckResponse(Status: "Healthy", Application: "BudgetFlow");

        return Ok(response);
    }
}
