using BudgetFlow.Domain.Enums;
using FluentValidation;

namespace BudgetFlow.Application.Features.Transactions;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Request.BankAccountId)
            .NotEmpty();

        RuleFor(x => x.Request.TransactionCategoryId)
            .NotEmpty();

        RuleFor(x => x.Request.TransactionType)
            .IsInEnum()
            .Must(type => type is TransactionType.Income or TransactionType.Expense)
            .WithMessage("Only Income and Expense transactions can be created directly. Use the transfer endpoint for transfers.");

        RuleFor(x => x.Request.Amount)
            .NotEqual(0)
            .WithMessage("Amount must not be zero.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(500);

        RuleFor(x => x.Request.ExternalReference)
            .MaximumLength(200);

        RuleFor(x => x.Request.OccurredAtUtc)
            .NotEmpty();
    }
}

public sealed class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty();

        RuleFor(x => x.Request.TransactionCategoryId)
            .NotEmpty();

        RuleFor(x => x.Request.Amount)
            .NotEqual(0)
            .WithMessage("Amount must not be zero.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(500);

        RuleFor(x => x.Request.OccurredAtUtc)
            .NotEmpty();
    }
}
