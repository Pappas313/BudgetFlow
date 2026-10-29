using BudgetFlow.Domain.Entities;
using BudgetFlow.Infrastructure.Identity;
using BudgetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BudgetFlow.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds the BudgetFlow database with realistic demo data for local development
/// and manual testing. This seeder is idempotent: it can be executed multiple
/// times without creating duplicate records, and it must never be invoked in a
/// non-Development environment.
/// </summary>
public static class DevelopmentDataSeeder
{
    private const string SeedMarker = "DevSeed:v1";
    private const string DefaultCurrency = "EUR";
    private const string DemoPassword = "Dev!Passw0rd123";

    private static readonly string[] AccountNames =
    [
        "Main Checking Account",
        "Savings Account",
        "Secondary Account",
        "Travel Fund Account",
        "Joint Household Account"
    ];

    private static readonly string[] IncomeCategoryNames =
    [
        "Salary",
        "Freelancing",
        "Investments",
        "Rental Income",
        "Gifts",
        "Other Income"
    ];

    private static readonly string[] ExpenseCategoryNames =
    [
        "Rent",
        "Groceries",
        "Electricity",
        "Internet",
        "Transportation",
        "Insurance",
        "Entertainment",
        "Healthcare",
        "Dining Out",
        "Clothing",
        "Education",
        "Subscriptions",
        "Miscellaneous"
    ];

    private static readonly string[] SavingsGoalNames =
    [
        "Emergency Fund",
        "Vacation to Portugal",
        "New Laptop",
        "Home Renovation",
        "Wedding Fund",
        "Car Replacement",
        "Retirement Boost"
    ];

    /// <summary>
    /// Seeds demo data into the database. This must only be called when the
    /// hosting environment is Development.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var dbContext = provider.GetRequiredService<BudgetFlowDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevelopmentDataSeeder));

        await dbContext.Database.MigrateAsync(cancellationToken);

        // Idempotency guard: if the seed marker already exists as an audit log entry,
        // the database has already been seeded and no further action is taken.
        var alreadySeeded = await dbContext.AuditLogs
            .AnyAsync(a => a.Action == SeedMarker, cancellationToken);

        if (alreadySeeded)
        {
            logger.LogInformation("Development data seed already applied. Skipping.");
            return;
        }

        logger.LogInformation("Seeding development data for BudgetFlow...");

        var random = new Random(20240101);
        var now = DateTimeOffset.UtcNow;
        var twelveMonthsAgo = now.AddMonths(-12);

        var users = await SeedUsersAsync(userManager, logger, cancellationToken);

        // 500 total transactions and 25 total bills are distributed evenly across the demo users.
        var transactionsPerUser = DistributeEvenly(500, users.Count);
        var billsPerUser = DistributeEvenly(25, users.Count);

        for (var userIndex = 0; userIndex < users.Count; userIndex++)
        {
            var user = users[userIndex];

            var accounts = await SeedBankAccountsAsync(dbContext, user.Id, random, cancellationToken);
            var incomeCategories = await SeedCategoriesAsync(dbContext, user.Id, IncomeCategoryNames, cancellationToken);
            var expenseCategories = await SeedCategoriesAsync(dbContext, user.Id, ExpenseCategoryNames, cancellationToken);

            await SeedTransactionsAsync(
                dbContext,
                accounts,
                incomeCategories,
                expenseCategories,
                random,
                twelveMonthsAgo,
                now,
                transactionsPerUser: transactionsPerUser[userIndex],
                cancellationToken);

            await SeedBillsAsync(dbContext, user.Id, expenseCategories, random, now, billsPerUser: billsPerUser[userIndex], cancellationToken);

            await SeedMonthlyBudgetsAsync(dbContext, user.Id, expenseCategories, random, now, cancellationToken);

            await SeedSavingsGoalsAsync(dbContext, user.Id, random, now, cancellationToken);
        }

        dbContext.AuditLogs.Add(new AuditLog(
            entityName: nameof(DevelopmentDataSeeder),
            entityId: SeedMarker,
            action: SeedMarker,
            changes: $"Development data seeded at {now:O}."));

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Development data seed completed successfully.");
    }

    private static int[] DistributeEvenly(int total, int bucketCount)
    {
        var result = new int[bucketCount];
        var baseAmount = total / bucketCount;
        var remainder = total % bucketCount;

        for (var i = 0; i < bucketCount; i++)
        {
            result[i] = baseAmount + (i < remainder ? 1 : 0);
        }

        return result;
    }

    private static async Task<List<ApplicationUser>> SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var demoUsers = new (string Email, string DisplayName)[]
        {
            ("demo.user1@budgetflow.local", "Demo User One"),
            ("demo.user2@budgetflow.local", "Demo User Two"),
        };

        var users = new List<ApplicationUser>();

        foreach (var (email, displayName) in demoUsers)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                users.Add(existing);
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to create demo user {Email}: {Errors}", email, errors);
                continue;
            }

            users.Add(user);
        }

        return users;
    }

    private static async Task<List<BankAccount>> SeedBankAccountsAsync(
        BudgetFlowDbContext dbContext,
        Guid applicationUserId,
        Random random,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.BankAccounts
            .Where(a => a.ApplicationUserId == applicationUserId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            return existing;
        }

        var accounts = AccountNames
            .Take(5)
            .Select(name => new BankAccount(applicationUserId, name, DefaultCurrency, institution: "BudgetFlow Demo Bank"))
            .ToList();

        dbContext.BankAccounts.AddRange(accounts);
        await dbContext.SaveChangesAsync(cancellationToken);

        return accounts;
    }

    private static async Task<List<TransactionCategory>> SeedCategoriesAsync(
        BudgetFlowDbContext dbContext,
        Guid applicationUserId,
        string[] categoryNames,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.TransactionCategories
            .Where(c => c.ApplicationUserId == applicationUserId && categoryNames.Contains(c.Name))
            .ToListAsync(cancellationToken);

        var missingNames = categoryNames.Except(existing.Select(c => c.Name)).ToList();

        if (missingNames.Count > 0)
        {
            var newCategories = missingNames
                .Select(name => new TransactionCategory(applicationUserId, name))
                .ToList();

            dbContext.TransactionCategories.AddRange(newCategories);
            await dbContext.SaveChangesAsync(cancellationToken);

            existing.AddRange(newCategories);
        }

        return existing;
    }

    private static async Task SeedTransactionsAsync(
        BudgetFlowDbContext dbContext,
        List<BankAccount> accounts,
        List<TransactionCategory> incomeCategories,
        List<TransactionCategory> expenseCategories,
        Random random,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int transactionsPerUser,
        CancellationToken cancellationToken)
    {
        var transactions = new List<FinancialTransaction>(transactionsPerUser);
        var balances = accounts.ToDictionary(a => a.Id, _ => 0m);

        for (var i = 0; i < transactionsPerUser; i++)
        {
            var account = accounts[random.Next(accounts.Count)];
            var isIncome = random.Next(100) < 30; // ~30% income, ~70% expense

            var category = isIncome
                ? incomeCategories[random.Next(incomeCategories.Count)]
                : expenseCategories[random.Next(expenseCategories.Count)];

            var amount = isIncome
                ? Math.Round((decimal)(random.NextDouble() * 2500 + 500), 2)
                : -Math.Round((decimal)(random.NextDouble() * 400 + 10), 2);

            var occurredAtUtc = startDate.AddDays(random.Next(0, (endDate - startDate).Days));

            var description = isIncome
                ? $"{category.Name} payment"
                : $"{category.Name} expense";

            var transactionType = isIncome
                ? BudgetFlow.Domain.Enums.TransactionType.Income
                : BudgetFlow.Domain.Enums.TransactionType.Expense;

            var transaction = new FinancialTransaction(account.Id, transactionType, amount, occurredAtUtc, category.Id, description);
            transactions.Add(transaction);

            balances[account.Id] += amount;
        }

        dbContext.FinancialTransactions.AddRange(transactions);

        // Reconcile bank account balances with the generated transaction history
        // so reported balances remain consistent with the transaction ledger.
        foreach (var account in accounts)
        {
            var closingBalance = Math.Round(1000m + balances[account.Id], 2);
            dbContext.Entry(account).Property(nameof(BankAccount.CurrentBalance)).CurrentValue = closingBalance;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedBillsAsync(
        BudgetFlowDbContext dbContext,
        Guid applicationUserId,
        List<TransactionCategory> expenseCategories,
        Random random,
        DateTimeOffset referenceDate,
        int billsPerUser,
        CancellationToken cancellationToken)
    {
        var existingCount = await dbContext.Bills
            .CountAsync(b => b.ApplicationUserId == applicationUserId, cancellationToken);

        if (existingCount > 0)
        {
            return;
        }

        var bills = new List<Bill>(billsPerUser);

        for (var i = 0; i < billsPerUser; i++)
        {
            var category = expenseCategories[random.Next(expenseCategories.Count)];
            var amount = Math.Round((decimal)(random.NextDouble() * 250 + 15), 2);
            var dueDate = referenceDate.AddDays(random.Next(1, 28));

            var bill = new Bill(applicationUserId, $"{category.Name} Bill", amount, dueDate, isRecurring: true);
            bills.Add(bill);
        }

        dbContext.Bills.AddRange(bills);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Record a payment for most bills to produce realistic payment history.
        var payments = bills
            .Where((_, index) => index % 4 != 0)
            .Select(bill => new BillPayment(bill.Id, bill.Amount, bill.DueDateUtc.AddDays(-random.Next(0, 5))))
            .ToList();

        dbContext.BillPayments.AddRange(payments);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMonthlyBudgetsAsync(
        BudgetFlowDbContext dbContext,
        Guid applicationUserId,
        List<TransactionCategory> expenseCategories,
        Random random,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken)
    {
        var existingCount = await dbContext.MonthlyBudgets
            .CountAsync(b => b.ApplicationUserId == applicationUserId, cancellationToken);

        if (existingCount > 0)
        {
            return;
        }

        var budgets = new List<MonthlyBudget>(12);
        var currentMonth = new DateTimeOffset(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < 12; i++)
        {
            var month = currentMonth.AddMonths(-i);
            var category = expenseCategories[random.Next(expenseCategories.Count)];
            var budgetedAmount = Math.Round((decimal)(random.NextDouble() * 500 + 100), 2);

            budgets.Add(new MonthlyBudget(applicationUserId, category.Id, month.Year, month.Month, budgetedAmount));
        }

        dbContext.MonthlyBudgets.AddRange(budgets);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSavingsGoalsAsync(
        BudgetFlowDbContext dbContext,
        Guid applicationUserId,
        Random random,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken)
    {
        var existingCount = await dbContext.SavingsGoals
            .CountAsync(g => g.ApplicationUserId == applicationUserId, cancellationToken);

        if (existingCount > 0)
        {
            return;
        }

        var goals = new List<SavingsGoal>(SavingsGoalNames.Length);

        foreach (var name in SavingsGoalNames)
        {
            var targetAmount = Math.Round((decimal)(random.NextDouble() * 8000 + 500), 2);
            var targetDate = referenceDate.AddMonths(random.Next(3, 24));

            goals.Add(new SavingsGoal(applicationUserId, name, targetAmount, targetDate));
        }

        dbContext.SavingsGoals.AddRange(goals);
        await dbContext.SaveChangesAsync(cancellationToken);

        var contributions = new List<SavingsContribution>();

        foreach (var goal in goals)
        {
            var contributionCount = random.Next(2, 6);
            var accumulated = 0m;

            for (var i = 0; i < contributionCount; i++)
            {
                var amount = Math.Round((decimal)(random.NextDouble() * 200 + 20), 2);
                var contributedAt = referenceDate.AddDays(-random.Next(1, 360));

                contributions.Add(new SavingsContribution(goal.Id, amount, contributedAt));
                accumulated += amount;
            }

            dbContext.Entry(goal).Property(nameof(SavingsGoal.CurrentAmount)).CurrentValue =
                Math.Round(Math.Min(accumulated, goal.TargetAmount), 2);
        }

        dbContext.SavingsContributions.AddRange(contributions);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
