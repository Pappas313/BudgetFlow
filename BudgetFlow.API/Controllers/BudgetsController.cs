using BudgetFlow.Application.Features.Budgets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Manages monthly category budgets, including allocation, utilization, and spending comparisons.
/// </summary>
[ApiController]
[Authorize]
[Route("api/budgets")]
[Produces("application/json")]
public sealed class BudgetsController : ControllerBase
{
    private readonly ISender _sender;

    public BudgetsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves the monthly budget summary (allocations vs. actuals) for the requested year and
    /// month, defaulting to the current month.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BudgetSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetSummaryDto>> GetAll(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMonthlyBudgetsQuery(year, month), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new monthly budget allocation for a category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MonthlyBudgetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MonthlyBudgetDto>> Create(
        [FromBody] CreateMonthlyBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateMonthlyBudgetCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates the allocated amount of an existing monthly budget.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MonthlyBudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MonthlyBudgetDto>> Update(
        Guid id,
        [FromBody] UpdateMonthlyBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateMonthlyBudgetCommand(id, request), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
