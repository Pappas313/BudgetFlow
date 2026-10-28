using Microsoft.AspNetCore.Identity;

namespace BudgetFlow.Infrastructure.Identity;

/// <summary>
/// Represents an application user built on top of ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
}
