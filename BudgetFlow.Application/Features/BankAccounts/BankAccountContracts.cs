using BudgetFlow.Domain.Enums;

namespace BudgetFlow.Application.Features.BankAccounts;

/// <summary>
/// Data transfer object representing a bank account owned by the current user.
/// </summary>
public sealed record BankAccountDto(
    Guid Id,
    string Name,
    string? Institution,
    AccountType AccountType,
    string Currency,
    decimal CurrentBalance,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

/// <summary>
/// Data transfer object representing the computed balance of a bank account.
/// </summary>
public sealed record AccountBalanceDto(Guid AccountId, string Currency, decimal Balance, DateTimeOffset AsOfUtc);

/// <summary>
/// Request payload used to create a new bank account.
/// </summary>
public sealed record CreateBankAccountRequest(
    string Name,
    string Currency,
    AccountType AccountType,
    string? Institution,
    decimal OpeningBalance);

/// <summary>
/// Request payload used to update an existing bank account's details.
/// </summary>
public sealed record UpdateBankAccountRequest(
    string Name,
    string Currency,
    AccountType AccountType,
    string? Institution);
