using BudgetFlow.Application.Features.BankAccounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Manages bank accounts owned by the authenticated user.
/// </summary>
[ApiController]
[Authorize]
[Route("api/accounts")]
[Produces("application/json")]
public sealed class BankAccountsController : ControllerBase
{
    private readonly ISender _sender;

    public BankAccountsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves all bank accounts owned by the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BankAccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BankAccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBankAccountsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single bank account by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BankAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BankAccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBankAccountByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new bank account for the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BankAccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BankAccountDto>> Create(
        [FromBody] CreateBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateBankAccountCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates the mutable details of an existing bank account.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BankAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BankAccountDto>> Update(
        Guid id,
        [FromBody] UpdateBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateBankAccountCommand(id, request), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Archives (soft-deletes) a bank account.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ArchiveBankAccountCommand(id), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retrieves the current computed balance of a bank account.
    /// </summary>
    [HttpGet("{id:guid}/balance")]
    [ProducesResponseType(typeof(AccountBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AccountBalanceDto>> GetBalance(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAccountBalanceQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
