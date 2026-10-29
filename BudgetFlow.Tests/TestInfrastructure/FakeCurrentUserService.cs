using BudgetFlow.Application.Abstractions;

namespace BudgetFlow.Tests.TestInfrastructure;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(Guid? userId)
    {
        UserId = userId;
    }

    public Guid? UserId { get; }

    public bool IsAuthenticated => UserId.HasValue;
}
