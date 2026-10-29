using FluentValidation;

namespace BudgetFlow.Application.Features.Transactions;

public sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.Request.SourceBankAccountId)
            .NotEmpty();

        RuleFor(x => x.Request.DestinationBankAccountId)
            .NotEmpty();

        RuleFor(x => x.Request)
            .Must(r => r.SourceBankAccountId != r.DestinationBankAccountId)
            .WithMessage("The source and destination accounts must be different.");

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(500);

        RuleFor(x => x.Request.ExternalReference)
            .MaximumLength(200);

        RuleFor(x => x.Request.OccurredAtUtc)
            .NotEmpty();
    }
}
