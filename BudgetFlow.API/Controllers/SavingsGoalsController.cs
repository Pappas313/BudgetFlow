using BudgetFlow.Application.Features.SavingsGoals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Manages savings goals and contributions made toward them by the authenticated user.
/// </summary>
[ApiController]
[Authorize]
[Route("api/savings-goals")]
[Produces("application/json")]
public sealed class SavingsGoalsController : ControllerBase
{
    private readonly ISender _sender;

    public SavingsGoalsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves all savings goals owned by the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SavingsGoalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SavingsGoalDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSavingsGoalsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new savings goal for the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SavingsGoalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SavingsGoalDto>> Create(
        [FromBody] CreateSavingsGoalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateSavingsGoalCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>
    /// Records a contribution toward a savings goal.
    /// </summary>
    [HttpPost("{id:guid}/contributions")]
    [ProducesResponseType(typeof(SavingsContributionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SavingsContributionDto>> RecordContribution(
        Guid id,
        [FromBody] RecordSavingsContributionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordSavingsContributionCommand(id, request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
