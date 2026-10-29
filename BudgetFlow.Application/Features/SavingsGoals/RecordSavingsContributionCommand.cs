using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.SavingsGoals;

/// <summary>
/// Command to record a contribution toward a savings goal owned by the current user.
/// </summary>
public sealed record RecordSavingsContributionCommand(Guid SavingsGoalId, RecordSavingsContributionRequest Request)
    : IRequest<SavingsContributionDto>;

public sealed class RecordSavingsContributionCommandHandler
    : IRequestHandler<RecordSavingsContributionCommand, SavingsContributionDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RecordSavingsContributionCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SavingsContributionDto> Handle(
        RecordSavingsContributionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to record a contribution.");

        var goal = await _dbContext.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == request.SavingsGoalId, cancellationToken)
            .ConfigureAwait(false);

        if (goal is null)
        {
            throw new NotFoundException(nameof(SavingsGoal), request.SavingsGoalId);
        }

        if (goal.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to contribute to this savings goal.");
        }

        var payload = request.Request;
        var contribution = new SavingsContribution(goal.Id, payload.Amount, payload.ContributedAtUtc);
        goal.RecordContribution(payload.Amount);

        _dbContext.SavingsContributions.Add(contribution);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return SavingsGoalMapper.ToDto(contribution);
    }
}
