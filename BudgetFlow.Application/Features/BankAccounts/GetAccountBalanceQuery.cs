using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Query to compute the current balance of a bank account, defined as the account's
/// opening balance plus the sum of all non-archived posted transactions.
/// </summary>
public sealed record GetAccountBalanceQuery(Guid AccountId) : IRequest<AccountBalanceDto>;

public sealed class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetAccountBalanceQueryHandler(
        IBudgetFlowDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AccountBalanceDto> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to view this bank account's balance.");

        var account = await _dbContext.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken)
            .ConfigureAwait(false);

        if (account is null)
        {
            throw new NotFoundException(nameof(BankAccount), request.AccountId);
        }

        if (account.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to view this bank account's balance.");
        }

        var transactionTotal = await _dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.BankAccountId == request.AccountId)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var balance = account.CurrentBalance + transactionTotal;

        return new AccountBalanceDto(account.Id, account.Currency, balance, _dateTimeProvider.UtcNow);
    }
}
