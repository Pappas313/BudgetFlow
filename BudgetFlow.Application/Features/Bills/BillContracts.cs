namespace BudgetFlow.Application.Features.Bills;

/// <summary>
/// Data transfer object representing a bill owed by the current user, including its derived
/// payment status for the current due cycle.
/// </summary>
public sealed record BillDto(
    Guid Id,
    string Name,
    decimal Amount,
    DateTimeOffset DueDateUtc,
    bool IsRecurring,
    bool IsPaid,
    bool IsOverdue,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

/// <summary>
/// Data transfer object representing a single payment recorded against a bill.
/// </summary>
public sealed record BillPaymentDto(Guid Id, Guid BillId, decimal AmountPaid, DateTimeOffset PaidAtUtc, DateTimeOffset CreatedAtUtc);

public sealed record CreateBillRequest(string Name, decimal Amount, DateTimeOffset DueDateUtc, bool IsRecurring);

public sealed record UpdateBillRequest(string Name, decimal Amount, DateTimeOffset DueDateUtc, bool IsRecurring);

public sealed record RecordBillPaymentRequest(decimal AmountPaid, DateTimeOffset PaidAtUtc);
