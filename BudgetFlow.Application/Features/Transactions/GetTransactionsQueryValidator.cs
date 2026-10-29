using FluentValidation;

namespace BudgetFlow.Application.Features.Transactions;

public sealed class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty();
    }
}

public sealed class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.Parameters.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Parameters.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Parameters.SortBy)
            .IsInEnum();

        RuleFor(x => x.Parameters)
            .Must(p => p.FromUtc is null || p.ToUtc is null || p.FromUtc <= p.ToUtc)
            .WithMessage("'FromUtc' must be less than or equal to 'ToUtc'.");
    }
}
