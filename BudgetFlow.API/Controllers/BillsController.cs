using BudgetFlow.Application.Features.Bills;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Manages bills owned by the authenticated user, including recurring bills, due dates, and
/// payment history.
/// </summary>
[ApiController]
[Authorize]
[Route("api/bills")]
[Produces("application/json")]
public sealed class BillsController : ControllerBase
{
    private readonly ISender _sender;

    public BillsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves all bills owned by the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BillDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBillsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves unpaid bills due within the next <paramref name="withinDays"/> days.
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IReadOnlyList<BillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BillDto>>> GetUpcoming(
        [FromQuery] int withinDays = 7,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetUpcomingBillsQuery(withinDays), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves bills whose due date has passed without a recorded payment.
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IReadOnlyList<BillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BillDto>>> GetOverdue(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOverdueBillsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new bill, optionally recurring.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BillDto>> Create([FromBody] CreateBillRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateBillCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates the mutable details of an existing bill.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BillDto>> Update(Guid id, [FromBody] UpdateBillRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateBillCommand(id, request), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Archives (soft-deletes) a bill, preserving its payment history.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteBillCommand(id), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Records a payment made against a bill.
    /// </summary>
    [HttpPost("{id:guid}/payments")]
    [ProducesResponseType(typeof(BillPaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BillPaymentDto>> RecordPayment(
        Guid id,
        [FromBody] RecordBillPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordBillPaymentCommand(id, request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
