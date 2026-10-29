using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Budgets;

/// <summary>
/// Command to update the allocated amount of an existing monthly budget owned by the current user.
/// </summary>
public sealed record UpdateMonthlyBudgetCommand(Guid BudgetId, UpdateMonthlyBudgetRequest Request) : IRequest<MonthlyBudgetDto>;

public sealed class UpdateMonthlyBudgetCommandHandler : IRequestHandler<UpdateMonthlyBudgetCommand, MonthlyBudgetDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMonthlyBudgetCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<MonthlyBudgetDto> Handle(UpdateMonthlyBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to update a budget.");

        var budget = await _dbContext.MonthlyBudgets
            .Include(b => b.TransactionCategory)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId, cancellationToken)
            .ConfigureAwait(false);

        if (budget is null)
        {
            throw new NotFoundException(nameof(MonthlyBudget), request.BudgetId);
        }

        if (budget.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to modify this budget.");
        }

        budget.UpdateBudgetedAmount(request.Request.BudgetedAmount);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var actualExpenses = await BudgetActualsCalculator
            .GetActualExpensesAsync(_dbContext, userId, budget.TransactionCategoryId, budget.Year, budget.Month, cancellationToken)
            .ConfigureAwait(false);

        return BudgetMapper.ToDto(budget, actualExpenses);
    }
}
