using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Query to retrieve a single bank account by its identifier.
/// </summary>
public sealed record GetBankAccountByIdQuery(Guid AccountId) : IRequest<BankAccountDto>;

public sealed class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetBankAccountByIdQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BankAccountDto> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to view this bank account.");

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
            throw new ForbiddenAccessException("You do not have permission to view this bank account.");
        }

        return BankAccountMapper.ToDto(account);
    }
}
