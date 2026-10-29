using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Budgets;

/// <summary>
/// Command to create a new monthly budget allocation for a category owned by the current user.
/// </summary>
public sealed record CreateMonthlyBudgetCommand(CreateMonthlyBudgetRequest Request) : IRequest<MonthlyBudgetDto>;

public sealed class CreateMonthlyBudgetCommandHandler : IRequestHandler<CreateMonthlyBudgetCommand, MonthlyBudgetDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateMonthlyBudgetCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<MonthlyBudgetDto> Handle(CreateMonthlyBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a budget.");

        var payload = request.Request;

        var category = await _dbContext.TransactionCategories
            .FirstOrDefaultAsync(c => c.Id == payload.TransactionCategoryId, cancellationToken)
            .ConfigureAwait(false);

        if (category is null || category.ApplicationUserId != userId)
        {
            throw new NotFoundException(nameof(TransactionCategory), payload.TransactionCategoryId);
        }

        var exists = await _dbContext.MonthlyBudgets
            .AnyAsync(
                b => b.ApplicationUserId == userId
                    && b.TransactionCategoryId == payload.TransactionCategoryId
                    && b.Year == payload.Year
                    && b.Month == payload.Month,
                cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            throw new ConflictException("A budget for this category and month already exists.");
        }

        var budget = new MonthlyBudget(userId, payload.TransactionCategoryId, payload.Year, payload.Month, payload.BudgetedAmount);

        _dbContext.MonthlyBudgets.Add(budget);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var actualExpenses = await BudgetActualsCalculator
            .GetActualExpensesAsync(_dbContext, userId, payload.TransactionCategoryId, payload.Year, payload.Month, cancellationToken)
            .ConfigureAwait(false);

        return BudgetMapper.ToDto(budget, actualExpenses) with { CategoryName = category.Name };
    }
}
