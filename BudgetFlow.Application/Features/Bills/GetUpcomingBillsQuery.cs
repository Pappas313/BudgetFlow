using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Query to retrieve bills owned by the current user that are due within the next
/// <see cref="WithinDays"/> days and have not yet been paid.
/// </summary>
public sealed record GetUpcomingBillsQuery(int WithinDays = 7) : IRequest<IReadOnlyList<BillDto>>;

public sealed class GetUpcomingBillsQueryHandler : IRequestHandler<GetUpcomingBillsQuery, IReadOnlyList<BillDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUpcomingBillsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BillDto>> Handle(GetUpcomingBillsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<BillDto>();
        }

        var utcNow = DateTimeOffset.UtcNow;
        var horizon = utcNow.AddDays(request.WithinDays);

        var bills = await _dbContext.Bills
            .AsNoTracking()
            .Include(b => b.BillPayments)
            .Where(b => b.ApplicationUserId == userId.Value && b.DueDateUtc >= utcNow && b.DueDateUtc <= horizon)
            .OrderBy(b => b.DueDateUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return bills
            .Select(b => BillMapper.ToDto(b, utcNow))
            .Where(dto => !dto.IsPaid)
            .ToList();
    }
}
