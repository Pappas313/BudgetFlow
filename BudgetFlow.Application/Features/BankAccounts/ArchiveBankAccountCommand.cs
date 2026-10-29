using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Command to archive (soft-delete) a bank account owned by the current user.
/// </summary>
public sealed record ArchiveBankAccountCommand(Guid AccountId) : IRequest;

public sealed class ArchiveBankAccountCommandHandler : IRequestHandler<ArchiveBankAccountCommand>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ArchiveBankAccountCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ArchiveBankAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to archive a bank account.");

        var account = await _dbContext.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken)
            .ConfigureAwait(false);

        if (account is null)
        {
            throw new NotFoundException(nameof(BankAccount), request.AccountId);
        }

        if (account.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to archive this bank account.");
        }

        account.Archive();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
