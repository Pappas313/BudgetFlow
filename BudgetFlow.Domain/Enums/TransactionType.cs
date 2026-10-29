namespace BudgetFlow.Domain.Enums;

/// <summary>
/// Classifies the nature of a <see cref="Entities.FinancialTransaction"/>.
/// </summary>
public enum TransactionType
{
    Income = 0,
    Expense = 1,
    Transfer = 2,
}
