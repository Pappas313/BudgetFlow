using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Command to create a new income or expense transaction on an account owned by the current user.
/// </summary>
public sealed record CreateTransactionCommand(CreateTransactionRequest Request) : IRequest<TransactionDto>;

public sealed class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateTransactionCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a transaction.");

        var payload = request.Request;

        var account = await _dbContext.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == payload.BankAccountId, cancellationToken)
            .ConfigureAwait(false);

        if (account is null)
        {
            throw new NotFoundException(nameof(BankAccount), payload.BankAccountId);
        }

        if (account.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to post transactions to this account.");
        }

        var category = await _dbContext.TransactionCategories
            .FirstOrDefaultAsync(c => c.Id == payload.TransactionCategoryId, cancellationToken)
            .ConfigureAwait(false);

        if (category is null)
        {
            throw new NotFoundException(nameof(TransactionCategory), payload.TransactionCategoryId);
        }

        if (category.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to use this transaction category.");
        }

        if (!string.IsNullOrWhiteSpace(payload.ExternalReference))
        {
            var isDuplicate = await _dbContext.FinancialTransactions
                .AnyAsync(
                    t => t.BankAccountId == payload.BankAccountId && t.ExternalReference == payload.ExternalReference,
                    cancellationToken)
                .ConfigureAwait(false);

            if (isDuplicate)
            {
                throw new ConflictException(
                    $"A transaction with external reference '{payload.ExternalReference}' has already been imported for this account.");
            }
        }

        var signedAmount = NormalizeAmount(payload.TransactionType, payload.Amount);

        var transaction = new FinancialTransaction(
            payload.BankAccountId,
            payload.TransactionType,
            signedAmount,
            payload.OccurredAtUtc,
            payload.TransactionCategoryId,
            payload.Description,
            payload.ExternalReference);

        _dbContext.FinancialTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        transaction = await _dbContext.FinancialTransactions
            .Include(t => t.TransactionCategory)
            .AsNoTracking()
            .FirstAsync(t => t.Id == transaction.Id, cancellationToken)
            .ConfigureAwait(false);

        return TransactionMapper.ToDto(transaction);
    }

    internal static decimal NormalizeAmount(TransactionType transactionType, decimal amount)
    {
        var magnitude = Math.Abs(amount);

        return transactionType switch
        {
            TransactionType.Income => magnitude,
            TransactionType.Expense => -magnitude,
            _ => throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, "Only Income and Expense transactions can be created directly."),
        };
    }
}
