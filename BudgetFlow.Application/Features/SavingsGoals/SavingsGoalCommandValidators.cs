using FluentValidation;

namespace BudgetFlow.Application.Features.SavingsGoals;

public sealed class CreateSavingsGoalCommandValidator : AbstractValidator<CreateSavingsGoalCommand>
{
    public CreateSavingsGoalCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.TargetAmount)
            .GreaterThan(0)
            .WithMessage("Target amount must be greater than zero.");
    }
}

public sealed class RecordSavingsContributionCommandValidator : AbstractValidator<RecordSavingsContributionCommand>
{
    public RecordSavingsContributionCommandValidator()
    {
        RuleFor(x => x.SavingsGoalId)
            .NotEmpty();

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Contribution amount must be greater than zero.");
    }
}
