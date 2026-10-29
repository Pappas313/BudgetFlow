using FluentValidation;

namespace BudgetFlow.Application.Features.Dashboard;

public sealed class GetDashboardSummaryQueryValidator : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryQueryValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue);
    }
}

public sealed class GetMonthlyTrendsQueryValidator : AbstractValidator<GetMonthlyTrendsQuery>
{
    public GetMonthlyTrendsQueryValidator()
    {
        RuleFor(x => x.MonthsBack)
            .GreaterThan(0)
            .LessThanOrEqualTo(60);
    }
}

public sealed class GetCategoryExpensesQueryValidator : AbstractValidator<GetCategoryExpensesQuery>
{
    public GetCategoryExpensesQueryValidator()
    {
        RuleFor(x => x.ToUtc)
            .GreaterThan(x => x.FromUtc)
            .When(x => x.FromUtc.HasValue && x.ToUtc.HasValue)
            .WithMessage("ToUtc must be later than FromUtc.");
    }
}

public sealed class GetBudgetOverviewQueryValidator : AbstractValidator<GetBudgetOverviewQuery>
{
    public GetBudgetOverviewQueryValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue);
    }
}

public sealed class GetDashboardUpcomingBillsQueryValidator : AbstractValidator<GetDashboardUpcomingBillsQuery>
{
    public GetDashboardUpcomingBillsQueryValidator()
    {
        RuleFor(x => x.WithinDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(366);
    }
}
