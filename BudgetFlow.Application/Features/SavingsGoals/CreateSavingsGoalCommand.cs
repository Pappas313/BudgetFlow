using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;

namespace BudgetFlow.Application.Features.SavingsGoals;

/// <summary>
/// Command to create a new savings goal owned by the current user.
/// </summary>
public sealed record CreateSavingsGoalCommand(CreateSavingsGoalRequest Request) : IRequest<SavingsGoalDto>;

public sealed class CreateSavingsGoalCommandHandler : IRequestHandler<CreateSavingsGoalCommand, SavingsGoalDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSavingsGoalCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SavingsGoalDto> Handle(CreateSavingsGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a savings goal.");

        var payload = request.Request;
        var goal = new SavingsGoal(userId, payload.Name, payload.TargetAmount, payload.TargetDateUtc);

        _dbContext.SavingsGoals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return SavingsGoalMapper.ToDto(goal);
    }
}
