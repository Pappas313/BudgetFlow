using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Query to retrieve all non-archived bank accounts owned by the current user.
/// </summary>
public sealed record GetBankAccountsQuery : IRequest<IReadOnlyList<BankAccountDto>>;

public sealed class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, IReadOnlyList<BankAccountDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetBankAccountsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BankAccountDto>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<BankAccountDto>();
        }

        var accounts = await _dbContext.BankAccounts
            .AsNoTracking()
            .Where(a => a.ApplicationUserId == userId.Value)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return accounts.Select(BankAccountMapper.ToDto).ToList();
    }
}
