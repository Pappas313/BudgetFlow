using BudgetFlow.Domain.Entities;

namespace BudgetFlow.Application.Features.Categories;

internal static class CategoryMapper
{
    public static TransactionCategoryDto ToDto(TransactionCategory category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.CreatedAtUtc);
}
