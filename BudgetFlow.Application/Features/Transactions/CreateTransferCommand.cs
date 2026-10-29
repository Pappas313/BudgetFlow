using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Command to transfer funds between two bank accounts owned by the current user. Creates two
/// linked <see cref="FinancialTransaction"/> legs (a debit on the source account and a credit on
/// the destination account) sharing a common <see cref="FinancialTransaction.TransferGroupId"/>,
/// persisted atomically within an explicit database transaction.
/// </summary>
public sealed record CreateTransferCommand(TransferRequest Request) : IRequest<TransferResultDto>;

public sealed class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, TransferResultDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateTransferCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransferResultDto> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a transfer.");

        var payload = request.Request;

        if (payload.SourceBankAccountId == payload.DestinationBankAccountId)
        {
            throw new ConflictException("The source and destination accounts must be different.");
        }

        var accounts = await _dbContext.BankAccounts
            .Where(a => a.Id == payload.SourceBankAccountId || a.Id == payload.DestinationBankAccountId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var sourceAccount = accounts.FirstOrDefault(a => a.Id == payload.SourceBankAccountId)
            ?? throw new NotFoundException(nameof(BankAccount), payload.SourceBankAccountId);

        var destinationAccount = accounts.FirstOrDefault(a => a.Id == payload.DestinationBankAccountId)
            ?? throw new NotFoundException(nameof(BankAccount), payload.DestinationBankAccountId);

        if (sourceAccount.ApplicationUserId != userId || destinationAccount.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to transfer funds using one or both of these accounts.");
        }

        if (!string.IsNullOrWhiteSpace(payload.ExternalReference))
        {
            var isDuplicate = await _dbContext.FinancialTransactions
                .AnyAsync(
                    t => t.BankAccountId == payload.SourceBankAccountId && t.ExternalReference == payload.ExternalReference,
                    cancellationToken)
                .ConfigureAwait(false);

            if (isDuplicate)
            {
                throw new ConflictException(
                    $"A transfer with external reference '{payload.ExternalReference}' has already been imported for this account.");
            }
        }

        var magnitude = Math.Abs(payload.Amount);
        var transferGroupId = Guid.NewGuid();

        var debitLeg = new FinancialTransaction(
            payload.SourceBankAccountId,
            TransactionType.Transfer,
            -magnitude,
            payload.OccurredAtUtc,
            description: payload.Description,
            externalReference: payload.ExternalReference,
            transferGroupId: transferGroupId);

        var creditLeg = new FinancialTransaction(
            payload.DestinationBankAccountId,
            TransactionType.Transfer,
            magnitude,
            payload.OccurredAtUtc,
            description: payload.Description,
            transferGroupId: transferGroupId);

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            _dbContext.FinancialTransactions.Add(debitLeg);
            _dbContext.FinancialTransactions.Add(creditLeg);

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        var persistedLegs = await _dbContext.FinancialTransactions
            .Include(t => t.TransactionCategory)
            .AsNoTracking()
            .Where(t => t.TransferGroupId == transferGroupId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var persistedDebit = persistedLegs.First(t => t.BankAccountId == payload.SourceBankAccountId);
        var persistedCredit = persistedLegs.First(t => t.BankAccountId == payload.DestinationBankAccountId);

        return new TransferResultDto(
            transferGroupId,
            TransactionMapper.ToDto(persistedDebit),
            TransactionMapper.ToDto(persistedCredit));
    }
}
