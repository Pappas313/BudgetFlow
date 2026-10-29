using FluentValidation;

namespace BudgetFlow.Application.Features.Bills;

public sealed class CreateBillCommandValidator : AbstractValidator<CreateBillCommand>
{
    public CreateBillCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Bill amount must be greater than zero.");

        RuleFor(x => x.Request.DueDateUtc)
            .NotEmpty();
    }
}

public sealed class UpdateBillCommandValidator : AbstractValidator<UpdateBillCommand>
{
    public UpdateBillCommandValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .WithMessage("Bill amount must be greater than zero.");

        RuleFor(x => x.Request.DueDateUtc)
            .NotEmpty();
    }
}

public sealed class DeleteBillCommandValidator : AbstractValidator<DeleteBillCommand>
{
    public DeleteBillCommandValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty();
    }
}

public sealed class RecordBillPaymentCommandValidator : AbstractValidator<RecordBillPaymentCommand>
{
    public RecordBillPaymentCommandValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty();

        RuleFor(x => x.Request.AmountPaid)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.Request.PaidAtUtc)
            .NotEmpty();
    }
}

public sealed class GetUpcomingBillsQueryValidator : AbstractValidator<GetUpcomingBillsQuery>
{
    public GetUpcomingBillsQueryValidator()
    {
        RuleFor(x => x.WithinDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(366);
    }
}
