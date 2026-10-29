using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Command to create a new bank account owned by the current user.
/// </summary>
public sealed record CreateBankAccountCommand(CreateBankAccountRequest Request) : IRequest<BankAccountDto>;

public sealed class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, BankAccountDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateBankAccountCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BankAccountDto> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a bank account.");

        var payload = request.Request;

        var account = new BankAccount(
            userId,
            payload.Name,
            payload.Currency.ToUpperInvariant(),
            payload.AccountType,
            payload.Institution,
            payload.OpeningBalance);

        _dbContext.BankAccounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BankAccountMapper.ToDto(account);
    }
}
