using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a payment made against a <see cref="Bill"/>.
/// </summary>
public class BillPayment : Entity, IAuditableEntity
{
    public Guid BillId { get; private set; }

    public Bill? Bill { get; private set; }

    public decimal AmountPaid { get; private set; }

    public DateTimeOffset PaidAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private BillPayment()
    {
    }

    public BillPayment(Guid billId, decimal amountPaid, DateTimeOffset paidAtUtc)
    {
        BillId = billId;
        AmountPaid = amountPaid;
        PaidAtUtc = paidAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
