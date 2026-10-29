using BudgetFlow.Domain.Entities;

namespace BudgetFlow.Application.Features.Transactions;

internal static class TransactionMapper
{
    public static TransactionDto ToDto(FinancialTransaction transaction) => new(
        transaction.Id,
        transaction.BankAccountId,
        transaction.TransactionCategoryId,
        transaction.TransactionCategory?.Name,
        transaction.TransactionType,
        transaction.Amount,
        transaction.Description,
        transaction.OccurredAtUtc,
        transaction.TransferGroupId,
        transaction.ExternalReference,
        transaction.CreatedAtUtc,
        transaction.LastModifiedAtUtc);
}
