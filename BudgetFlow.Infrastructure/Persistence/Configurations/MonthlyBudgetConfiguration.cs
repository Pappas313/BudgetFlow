using BudgetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetFlow.Infrastructure.Persistence.Configurations;

public class MonthlyBudgetConfiguration : IEntityTypeConfiguration<MonthlyBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyBudget> builder)
    {
        builder.ToTable("MonthlyBudgets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BudgetedAmount)
            .HasColumnType("decimal(19,4)")
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.ApplicationUserId, x.TransactionCategoryId, x.Year, x.Month })
            .IsUnique();

        builder.HasQueryFilter(x => x.TransactionCategory == null || !x.TransactionCategory.IsDeleted);

        builder.HasOne(x => x.TransactionCategory)
            .WithMany()
            .HasForeignKey(x => x.TransactionCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
