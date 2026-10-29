using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;

namespace BudgetFlow.Application.Features.Categories;

/// <summary>
/// Command to create a new transaction category owned by the current user.
/// </summary>
public sealed record CreateTransactionCategoryCommand(CreateTransactionCategoryRequest Request)
    : IRequest<TransactionCategoryDto>;

public sealed class CreateTransactionCategoryCommandHandler
    : IRequestHandler<CreateTransactionCategoryCommand, TransactionCategoryDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateTransactionCategoryCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionCategoryDto> Handle(
        CreateTransactionCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a category.");

        var payload = request.Request;
        var category = new TransactionCategory(userId, payload.Name, payload.Description);

        _dbContext.TransactionCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryMapper.ToDto(category);
    }
}
