using BudgetFlow.Application.Features.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Manages transaction categories used to classify income, expenses, and budgets.
/// </summary>
[ApiController]
[Authorize]
[Route("api/categories")]
[Produces("application/json")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves all transaction categories owned by the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionCategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionCategoriesQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new transaction category for the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionCategoryDto>> Create(
        [FromBody] CreateTransactionCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateTransactionCategoryCommand(request), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
