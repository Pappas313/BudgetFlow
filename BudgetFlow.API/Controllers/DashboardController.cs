using BudgetFlow.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Controllers;

/// <summary>
/// Provides aggregated financial dashboard data for the authenticated user: account balances,
/// income/expense trends, category breakdowns, budget utilization, and upcoming bill reminders.
/// </summary>
[ApiController]
[Authorize]
[Route("api/dashboard")]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves the high-level financial summary: total account balances, monthly income,
    /// monthly expenses, net cash flow, overall budget utilization, and savings progress.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDashboardSummaryQuery(year, month), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves income/expense/net cash flow trends over the trailing <paramref name="monthsBack"/> months.
    /// </summary>
    [HttpGet("monthly-trends")]
    [ProducesResponseType(typeof(IReadOnlyList<MonthlyTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MonthlyTrendDto>>> GetMonthlyTrends(
        [FromQuery] int monthsBack = 12,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetMonthlyTrendsQuery(monthsBack), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves total expenses per category within an optional date range (defaults to the
    /// current calendar month).
    /// </summary>
    [HttpGet("category-expenses")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryExpenseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryExpenseDto>>> GetCategoryExpenses(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCategoryExpensesQuery(fromUtc, toUtc), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves budget allocation vs. actual spending per category for the requested month
    /// (defaults to the current month).
    /// </summary>
    [HttpGet("budget-overview")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetOverviewItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BudgetOverviewItemDto>>> GetBudgetOverview(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBudgetOverviewQuery(year, month), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves unpaid bills due within the next <paramref name="withinDays"/> days.
    /// </summary>
    [HttpGet("upcoming-bills")]
    [ProducesResponseType(typeof(IReadOnlyList<UpcomingBillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UpcomingBillDto>>> GetUpcomingBills(
        [FromQuery] int withinDays = 7,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetDashboardUpcomingBillsQuery(withinDays), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
