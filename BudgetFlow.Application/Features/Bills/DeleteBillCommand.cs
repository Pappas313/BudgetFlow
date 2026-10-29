using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Command to archive (soft-delete) a bill owned by the current user.
/// </summary>
public sealed record DeleteBillCommand(Guid BillId) : IRequest;

public sealed class DeleteBillCommandHandler : IRequestHandler<DeleteBillCommand>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBillCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteBillCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to delete a bill.");

        var bill = await _dbContext.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            .ConfigureAwait(false);

        if (bill is null)
        {
            throw new NotFoundException(nameof(Bill), request.BillId);
        }

        if (bill.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to delete this bill.");
        }

        bill.Archive();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
