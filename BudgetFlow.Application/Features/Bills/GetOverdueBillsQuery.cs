using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Query to retrieve bills owned by the current user whose due date has passed without a
/// corresponding payment being recorded.
/// </summary>
public sealed record GetOverdueBillsQuery : IRequest<IReadOnlyList<BillDto>>;

public sealed class GetOverdueBillsQueryHandler : IRequestHandler<GetOverdueBillsQuery, IReadOnlyList<BillDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetOverdueBillsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BillDto>> Handle(GetOverdueBillsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<BillDto>();
        }

        var utcNow = DateTimeOffset.UtcNow;

        var bills = await _dbContext.Bills
            .AsNoTracking()
            .Include(b => b.BillPayments)
            .Where(b => b.ApplicationUserId == userId.Value && b.DueDateUtc < utcNow)
            .OrderBy(b => b.DueDateUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return bills
            .Select(b => BillMapper.ToDto(b, utcNow))
            .Where(dto => dto.IsOverdue)
            .ToList();
    }
}
