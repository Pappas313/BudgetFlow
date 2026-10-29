using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Exceptions;
using BudgetFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Command to record a payment against a bill owned by the current user.
/// </summary>
public sealed record RecordBillPaymentCommand(Guid BillId, RecordBillPaymentRequest Request) : IRequest<BillPaymentDto>;

public sealed class RecordBillPaymentCommandHandler : IRequestHandler<RecordBillPaymentCommand, BillPaymentDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RecordBillPaymentCommandHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BillPaymentDto> Handle(RecordBillPaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException("You must be authenticated to record a bill payment.");

        var bill = await _dbContext.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            .ConfigureAwait(false);

        if (bill is null)
        {
            throw new NotFoundException(nameof(Bill), request.BillId);
        }

        if (bill.ApplicationUserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to record payments for this bill.");
        }

        var payload = request.Request;
        var payment = new BillPayment(bill.Id, payload.AmountPaid, payload.PaidAtUtc);

        _dbContext.BillPayments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BillMapper.ToDto(payment);
    }
}
