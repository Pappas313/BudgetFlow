using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Categories;

/// <summary>
/// Query to retrieve all non-archived transaction categories owned by the current user.
/// </summary>
public sealed record GetTransactionCategoriesQuery : IRequest<IReadOnlyList<TransactionCategoryDto>>;

public sealed class GetTransactionCategoriesQueryHandler
    : IRequestHandler<GetTransactionCategoriesQuery, IReadOnlyList<TransactionCategoryDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionCategoriesQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<TransactionCategoryDto>> Handle(
        GetTransactionCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<TransactionCategoryDto>();
        }

        var categories = await _dbContext.TransactionCategories
            .AsNoTracking()
            .Where(c => c.ApplicationUserId == userId.Value)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return categories.Select(CategoryMapper.ToDto).ToList();
    }
}
