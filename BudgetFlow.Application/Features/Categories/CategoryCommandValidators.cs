using FluentValidation;

namespace BudgetFlow.Application.Features.Categories;

public sealed class CreateTransactionCategoryCommandValidator : AbstractValidator<CreateTransactionCategoryCommand>
{
    public CreateTransactionCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Description)
            .MaximumLength(500);
    }
}
