using BudgetFlow.Domain.Entities;

namespace BudgetFlow.Application.Features.BankAccounts;

internal static class BankAccountMapper
{
    public static BankAccountDto ToDto(BankAccount account) => new(
        account.Id,
        account.Name,
        account.Institution,
        account.AccountType,
        account.Currency,
        account.CurrentBalance,
        account.CreatedAtUtc,
        account.LastModifiedAtUtc);
}
