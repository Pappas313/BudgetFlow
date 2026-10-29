using FluentValidation;

namespace BudgetFlow.Application.Features.BankAccounts;

public sealed class GetBankAccountByIdQueryValidator : AbstractValidator<GetBankAccountByIdQuery>
{
    public GetBankAccountByIdQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();
    }
}

public sealed class GetAccountBalanceQueryValidator : AbstractValidator<GetAccountBalanceQuery>
{
    public GetAccountBalanceQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();
    }
}
