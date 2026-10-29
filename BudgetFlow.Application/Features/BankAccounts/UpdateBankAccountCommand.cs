using BudgetFlow.Domain.Entities;
using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Command to update the mutable details of an existing bank account owned by the current user.
/// </summary>
public sealed record UpdateBankAccountCommand(Guid AccountId, UpdateBankAccountRequest Request) : IRequest<BankAccountDto>;

public sealed class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, BankAccountDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBankAccountCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BankAccountDto> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to update a bank account.");

        var account = await _dbContext.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken)
            .ConfigureAwait(false);

        if (account is null)
        {
            throw new NotFoundException(nameof(BankAccount), request.AccountId);
        }

        if (account.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to modify this bank account.");
        }

        var payload = request.Request;
        account.UpdateDetails(payload.Name, payload.Currency.ToUpperInvariant(), payload.AccountType, payload.Institution);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BankAccountMapper.ToDto(account);
    }
}
