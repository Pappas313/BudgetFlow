using BudgetFlow.Domain.Enums;
using FluentValidation;

namespace BudgetFlow.Application.Features.BankAccounts;

public sealed class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Currency)
            .NotEmpty()
            .Length(3)
            .Must(currency => currency.All(char.IsLetter))
            .WithMessage("Currency must be a valid 3-letter ISO 4217 currency code.");

        RuleFor(x => x.Request.AccountType)
            .IsInEnum();

        RuleFor(x => x.Request.Institution)
            .MaximumLength(200);

        RuleFor(x => x.Request.OpeningBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Opening balance cannot be negative.");
    }
}

public sealed class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
{
    public UpdateBankAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.Request.AccountType)
            .IsInEnum();

        RuleFor(x => x.Request.Institution)
            .MaximumLength(200);
    }
}

public sealed class ArchiveBankAccountCommandValidator : AbstractValidator<ArchiveBankAccountCommand>
{
    public ArchiveBankAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();
    }
}
