using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Command to update the mutable details of an existing income or expense transaction.
/// Transfer legs cannot be updated directly; they must be deleted and re-created.
/// </summary>
public sealed record UpdateTransactionCommand(Guid TransactionId, UpdateTransactionRequest Request) : IRequest<TransactionDto>;

public sealed class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTransactionCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to update a transaction.");

        var transaction = await _dbContext.FinancialTransactions
            .Include(t => t.BankAccount)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            .ConfigureAwait(false);

        if (transaction is null)
        {
            throw new NotFoundException(nameof(FinancialTransaction), request.TransactionId);
        }

        if (transaction.BankAccount?.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to modify this transaction.");
        }

        if (transaction.TransactionType == TransactionType.Transfer)
        {
            throw new ConflictException("Transfer transactions cannot be edited directly. Delete and re-create the transfer instead.");
        }

        var payload = request.Request;

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

        var signedAmount = CreateTransactionCommandHandler.NormalizeAmount(transaction.TransactionType, payload.Amount);

        transaction.UpdateDetails(payload.TransactionCategoryId, signedAmount, payload.OccurredAtUtc, payload.Description);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        transaction = await _dbContext.FinancialTransactions
            .Include(t => t.TransactionCategory)
            .AsNoTracking()
            .FirstAsync(t => t.Id == transaction.Id, cancellationToken)
            .ConfigureAwait(false);

        return TransactionMapper.ToDto(transaction);
    }
}
