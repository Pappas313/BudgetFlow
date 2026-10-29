using BudgetFlow.Domain.Entities;

namespace BudgetFlow.Application.Features.Bills;

internal static class BillMapper
{
    /// <summary>
    /// A bill is considered paid for its current due cycle if any recorded payment occurred on
    /// or after the due date. Overdue means the due date has passed and no such payment exists.
    /// </summary>
    public static BillDto ToDto(Bill bill, DateTimeOffset utcNow)
    {
        var isPaid = bill.BillPayments.Any(p => p.PaidAtUtc >= bill.DueDateUtc);
        var isOverdue = !isPaid && bill.DueDateUtc < utcNow;

        return new BillDto(
            bill.Id,
            bill.Name,
            bill.Amount,
            bill.DueDateUtc,
            bill.IsRecurring,
            isPaid,
            isOverdue,
            bill.CreatedAtUtc,
            bill.LastModifiedAtUtc);
    }

    public static BillPaymentDto ToDto(BillPayment payment) => new(
        payment.Id,
        payment.BillId,
        payment.AmountPaid,
        payment.PaidAtUtc,
        payment.CreatedAtUtc);
}
