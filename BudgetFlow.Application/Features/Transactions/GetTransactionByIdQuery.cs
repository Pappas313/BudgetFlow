using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Query to retrieve a single transaction by its identifier.
/// </summary>
public sealed record GetTransactionByIdQuery(Guid TransactionId) : IRequest<TransactionDto>;

public sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionByIdQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to view this transaction.");

        var transaction = await _dbContext.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.TransactionCategory)
            .Include(t => t.BankAccount)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            .ConfigureAwait(false);

        if (transaction is null)
        {
            throw new NotFoundException(nameof(FinancialTransaction), request.TransactionId);
        }

        if (transaction.BankAccount?.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to view this transaction.");
        }

        return TransactionMapper.ToDto(transaction);
    }
}
