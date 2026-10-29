using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Query to retrieve a paginated, filtered, and sorted list of transactions for accounts owned
/// by the current user.
/// </summary>
public sealed record GetTransactionsQuery(TransactionQueryParameters Parameters) : IRequest<PagedResult<TransactionDto>>;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var parameters = request.Parameters;

        if (userId is null)
        {
            return new PagedResult<TransactionDto>(Array.Empty<TransactionDto>(), 0, parameters.Page, parameters.PageSize);
        }

        var query = _dbContext.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.TransactionCategory)
            .Where(t => t.BankAccount!.ApplicationUserId == userId.Value);

        if (parameters.BankAccountId is Guid accountId)
        {
            query = query.Where(t => t.BankAccountId == accountId);
        }

        if (parameters.TransactionCategoryId is Guid categoryId)
        {
            query = query.Where(t => t.TransactionCategoryId == categoryId);
        }

        if (parameters.TransactionType is not null)
        {
            query = query.Where(t => t.TransactionType == parameters.TransactionType);
        }

        if (parameters.FromUtc is DateTimeOffset fromUtc)
        {
            query = query.Where(t => t.OccurredAtUtc >= fromUtc);
        }

        if (parameters.ToUtc is DateTimeOffset toUtc)
        {
            query = query.Where(t => t.OccurredAtUtc <= toUtc);
        }

        query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(TransactionMapper.ToDto).ToList();

        return new PagedResult<TransactionDto>(dtos, totalCount, page, pageSize);
    }

    private static IQueryable<Domain.Entities.FinancialTransaction> ApplySorting(
        IQueryable<Domain.Entities.FinancialTransaction> query,
        TransactionSortField sortBy,
        bool sortDescending)
    {
        return sortBy switch
        {
            TransactionSortField.Amount => sortDescending
                ? query.OrderByDescending(t => t.Amount)
                : query.OrderBy(t => t.Amount),
            TransactionSortField.CreatedAtUtc => sortDescending
                ? query.OrderByDescending(t => t.CreatedAtUtc)
                : query.OrderBy(t => t.CreatedAtUtc),
            _ => sortDescending
                ? query.OrderByDescending(t => t.OccurredAtUtc)
                : query.OrderBy(t => t.OccurredAtUtc),
        };
    }
}
