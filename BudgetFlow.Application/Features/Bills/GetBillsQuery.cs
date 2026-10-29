using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Query to retrieve all non-archived bills owned by the current user.
/// </summary>
public sealed record GetBillsQuery : IRequest<IReadOnlyList<BillDto>>;

public sealed class GetBillsQueryHandler : IRequestHandler<GetBillsQuery, IReadOnlyList<BillDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetBillsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BillDto>> Handle(GetBillsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<BillDto>();
        }

        var bills = await _dbContext.Bills
            .AsNoTracking()
            .Include(b => b.BillPayments)
            .Where(b => b.ApplicationUserId == userId.Value)
            .OrderBy(b => b.DueDateUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var utcNow = DateTimeOffset.UtcNow;
        return bills.Select(b => BillMapper.ToDto(b, utcNow)).ToList();
    }
}
