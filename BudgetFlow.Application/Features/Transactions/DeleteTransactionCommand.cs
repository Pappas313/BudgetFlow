using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Command to archive (soft-delete) a transaction. If the transaction is a transfer leg, both
/// legs sharing the same <see cref="FinancialTransaction.TransferGroupId"/> are archived together.
/// </summary>
public sealed record DeleteTransactionCommand(Guid TransactionId) : IRequest;

public sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTransactionCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to delete a transaction.");

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
            throw new ForbiddenAccessException("You do not have permission to delete this transaction.");
        }

        if (transaction.TransferGroupId is Guid transferGroupId)
        {
            var legs = await _dbContext.FinancialTransactions
                .Include(t => t.BankAccount)
                .Where(t => t.TransferGroupId == transferGroupId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var leg in legs)
            {
                if (leg.BankAccount?.ApplicationUserId != userId)
                {
                    throw new ForbiddenAccessException("You do not have permission to delete this transfer.");
                }

                leg.Archive();
            }
        }
        else
        {
            transaction.Archive();
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
