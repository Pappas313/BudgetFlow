using BudgetFlow.Application.Common;
using BudgetFlow.Application.Features.Transactions;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Manages financial transactions (income, expense, and transfers) for accounts owned by the
/// authenticated user.
/// </summary>
[ApiController]
[Authorize]
[Route("api/transactions")]
[Produces("application/json")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public TransactionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated, filtered, and sorted list of transactions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetAll(
        [FromQuery] Guid? bankAccountId,
        [FromQuery] Guid? transactionCategoryId,
        [FromQuery] TransactionType? transactionType,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] TransactionSortField sortBy = TransactionSortField.OccurredAtUtc,
        [FromQuery] bool sortDescending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var parameters = new TransactionQueryParameters(
            bankAccountId,
            transactionCategoryId,
            transactionType,
            fromUtc,
            toUtc,
            sortBy,
            sortDescending,
            page,
            pageSize);

        var result = await _sender.Send(new GetTransactionsQuery(parameters), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single transaction by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new income or expense transaction.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionDto>> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateTransactionCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates the mutable details of an existing income or expense transaction.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionDto>> Update(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateTransactionCommand(id, request), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Deletes (archives) a transaction. Deleting a transfer leg archives both linked legs.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTransactionCommand(id), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Transfers funds between two accounts owned by the current user, posted atomically.
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransferResultDto>> Transfer(
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateTransferCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id = result.DebitTransaction.Id }, result);
    }

    /// <summary>
    /// Retrieves aggregated income/expense totals, excluding transfers.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(TransactionSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionSummaryDto>> GetSummary(
        [FromQuery] Guid? bankAccountId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionSummaryQuery(bankAccountId, fromUtc, toUtc), cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }
}
