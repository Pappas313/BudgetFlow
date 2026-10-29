using BudgetFlow.Domain.Enums;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Data transfer object representing a single financial transaction (income, expense, or transfer leg).
/// </summary>
public sealed record TransactionDto(
    Guid Id,
    Guid BankAccountId,
    Guid? TransactionCategoryId,
    string? CategoryName,
    TransactionType TransactionType,
    decimal Amount,
    string? Description,
    DateTimeOffset OccurredAtUtc,
    Guid? TransferGroupId,
    string? ExternalReference,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

/// <summary>
/// Request payload used to create a new income or expense transaction. Transfers are created via
/// the dedicated transfer endpoint/command instead.
/// </summary>
public sealed record CreateTransactionRequest(
    Guid BankAccountId,
    Guid TransactionCategoryId,
    TransactionType TransactionType,
    decimal Amount,
    string? Description,
    DateTimeOffset OccurredAtUtc,
    string? ExternalReference);

/// <summary>
/// Request payload used to update the mutable details of an existing income or expense transaction.
/// </summary>
public sealed record UpdateTransactionRequest(
    Guid TransactionCategoryId,
    decimal Amount,
    string? Description,
    DateTimeOffset OccurredAtUtc);

/// <summary>
/// Request payload used to create a transfer between two bank accounts owned by the current user.
/// </summary>
public sealed record TransferRequest(
    Guid SourceBankAccountId,
    Guid DestinationBankAccountId,
    decimal Amount,
    string? Description,
    DateTimeOffset OccurredAtUtc,
    string? ExternalReference);

/// <summary>
/// Result of a successfully created transfer, exposing both legs.
/// </summary>
public sealed record TransferResultDto(Guid TransferGroupId, TransactionDto DebitTransaction, TransactionDto CreditTransaction);

/// <summary>
/// Aggregated income/expense totals over an (optional) date range and/or account, excluding transfers.
/// </summary>
public sealed record TransactionSummaryDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetAmount,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);

public enum TransactionSortField
{
    OccurredAtUtc = 0,
    Amount = 1,
    CreatedAtUtc = 2,
}

/// <summary>
/// Filtering, sorting, and pagination parameters for listing transactions.
/// </summary>
public sealed record TransactionQueryParameters(
    Guid? BankAccountId,
    Guid? TransactionCategoryId,
    TransactionType? TransactionType,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    TransactionSortField SortBy,
    bool SortDescending,
    int Page,
    int PageSize);
