using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Command to update the mutable details of an existing bill owned by the current user.
/// </summary>
public sealed record UpdateBillCommand(Guid BillId, UpdateBillRequest Request) : IRequest<BillDto>;

public sealed class UpdateBillCommandHandler : IRequestHandler<UpdateBillCommand, BillDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBillCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BillDto> Handle(UpdateBillCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to update a bill.");

        var bill = await _dbContext.Bills
            .Include(b => b.BillPayments)
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            .ConfigureAwait(false);

        if (bill is null)
        {
            throw new NotFoundException(nameof(Bill), request.BillId);
        }

        if (bill.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to modify this bill.");
        }

        var payload = request.Request;
        bill.UpdateDetails(payload.Name, payload.Amount, payload.DueDateUtc, payload.IsRecurring);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BillMapper.ToDto(bill, DateTimeOffset.UtcNow);
    }
}
