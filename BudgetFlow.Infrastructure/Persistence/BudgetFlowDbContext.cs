using BudgetFlow.Application.Abstractions;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BudgetFlow.Infrastructure.Persistence;

/// <summary>
/// The primary EF Core database context for BudgetFlow, combining ASP.NET Core
/// Identity storage with the application's domain persistence model.
/// </summary>
public class BudgetFlowDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IBudgetFlowDbContext
{
    public BudgetFlowDbContext(DbContextOptions<BudgetFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();

    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();

    public DbSet<Bill> Bills => Set<Bill>();

    public DbSet<BillPayment> BillPayments => Set<BillPayment>();

    public DbSet<MonthlyBudget> MonthlyBudgets => Set<MonthlyBudget>();

    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();

    public DbSet<SavingsContribution> SavingsContributions => Set<SavingsContribution>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<ReportExport> ReportExports => Set<ReportExport>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(BudgetFlowDbContext).Assembly);
    }
}
