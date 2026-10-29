namespace BudgetFlow.Domain.Enums;

/// <summary>
/// Classifies the kind of financial product a <see cref="Entities.BankAccount"/> represents.
/// </summary>
public enum AccountType
{
    Checking = 0,
    Savings = 1,
    Credit = 2,
    Investment = 3,
    Cash = 4,
    Other = 5,
}
