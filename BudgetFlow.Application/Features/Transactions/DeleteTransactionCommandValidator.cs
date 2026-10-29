using FluentValidation;

namespace BudgetFlow.Application.Features.Transactions;

public sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty();
    }
}
