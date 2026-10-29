using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Dashboard;

/// <summary>
/// Query to retrieve unpaid bills owned by the current user due within the next
/// <see cref="WithinDays"/> days, for the dashboard reminders widget.
/// </summary>
public sealed record GetDashboardUpcomingBillsQuery(int WithinDays = 7) : IRequest<IReadOnlyList<UpcomingBillDto>>;

public sealed class GetDashboardUpcomingBillsQueryHandler
    : IRequestHandler<GetDashboardUpcomingBillsQuery, IReadOnlyList<UpcomingBillDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardUpcomingBillsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<UpcomingBillDto>> Handle(GetDashboardUpcomingBillsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<UpcomingBillDto>();
        }

        var utcNow = DateTimeOffset.UtcNow;
        var horizon = utcNow.AddDays(request.WithinDays);

        var bills = await _dbContext.Bills
            .AsNoTracking()
            .Include(b => b.BillPayments)
            .Where(b => b.ApplicationUserId == userId.Value && b.DueDateUtc <= horizon)
            .OrderBy(b => b.DueDateUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return bills
            .Where(b => !b.BillPayments.Any(p => p.PaidAtUtc >= b.DueDateUtc))
            .Select(b => new UpcomingBillDto(b.Id, b.Name, b.Amount, b.DueDateUtc, b.DueDateUtc < utcNow))
            .ToList();
    }
}
