using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.SavingsGoals;

/// <summary>
/// Query to retrieve all non-archived savings goals owned by the current user.
/// </summary>
public sealed record GetSavingsGoalsQuery : IRequest<IReadOnlyList<SavingsGoalDto>>;

public sealed class GetSavingsGoalsQueryHandler : IRequestHandler<GetSavingsGoalsQuery, IReadOnlyList<SavingsGoalDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSavingsGoalsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<SavingsGoalDto>> Handle(GetSavingsGoalsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<SavingsGoalDto>();
        }

        var goals = await _dbContext.SavingsGoals
            .AsNoTracking()
            .Where(g => g.ApplicationUserId == userId.Value)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return goals.Select(SavingsGoalMapper.ToDto).ToList();
    }
}
