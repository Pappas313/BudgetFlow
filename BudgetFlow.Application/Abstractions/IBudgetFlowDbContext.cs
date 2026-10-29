using BudgetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BudgetFlow.Application.Abstractions;

/// <summary>
/// Abstraction over the application's persistence context, exposed to the
/// Application layer so that MediatR handlers can query and persist domain
/// entities without taking a dependency on the Infrastructure layer.
/// </summary>
public interface IBudgetFlowDbContext
{
    DbSet<BankAccount> BankAccounts { get; }

    DbSet<FinancialTransaction> FinancialTransactions { get; }

    DbSet<TransactionCategory> TransactionCategories { get; }

    DbSet<Bill> Bills { get; }

    DbSet<BillPayment> BillPayments { get; }

    DbSet<MonthlyBudget> MonthlyBudgets { get; }

    DbSet<SavingsGoal> SavingsGoals { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
