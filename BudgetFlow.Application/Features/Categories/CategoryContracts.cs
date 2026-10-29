namespace BudgetFlow.Application.Features.Categories;

/// <summary>
/// Data transfer object representing a transaction category owned by the current user.
/// </summary>
public sealed record TransactionCategoryDto(Guid Id, string Name, string? Description, DateTimeOffset CreatedAtUtc);

/// <summary>
/// Request payload used to create a new transaction category.
/// </summary>
public sealed record CreateTransactionCategoryRequest(string Name, string? Description);
