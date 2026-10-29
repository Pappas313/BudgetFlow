using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Command to create a new bill owned by the current user.
/// </summary>
public sealed record CreateBillCommand(CreateBillRequest Request) : IRequest<BillDto>;

public sealed class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, BillDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateBillCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BillDto> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to create a bill.");

        var payload = request.Request;

        var bill = new Bill(userId, payload.Name, payload.Amount, payload.DueDateUtc, payload.IsRecurring);

        _dbContext.Bills.Add(bill);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BillMapper.ToDto(bill, DateTimeOffset.UtcNow);
    }
}
