using FluentValidation;

namespace BudgetFlow.Application.Features.Budgets;

public sealed class CreateMonthlyBudgetCommandValidator : AbstractValidator<CreateMonthlyBudgetCommand>
{
    public CreateMonthlyBudgetCommandValidator()
    {
        RuleFor(x => x.Request.TransactionCategoryId)
            .NotEmpty();

        RuleFor(x => x.Request.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Request.Month)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.Request.BudgetedAmount)
            .GreaterThan(0)
            .WithMessage("Budgeted amount must be greater than zero.");
    }
}

public sealed class UpdateMonthlyBudgetCommandValidator : AbstractValidator<UpdateMonthlyBudgetCommand>
{
    public UpdateMonthlyBudgetCommandValidator()
    {
        RuleFor(x => x.BudgetId)
            .NotEmpty();

        RuleFor(x => x.Request.BudgetedAmount)
            .GreaterThan(0)
            .WithMessage("Budgeted amount must be greater than zero.");
    }
}
