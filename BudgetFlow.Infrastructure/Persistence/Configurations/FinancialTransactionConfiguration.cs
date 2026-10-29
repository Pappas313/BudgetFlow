using BudgetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetFlow.Infrastructure.Persistence.Configurations;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(19,4)")
            .IsRequired();

        builder.Property(x => x.TransactionType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(200);

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.BankAccountId);
        builder.HasIndex(x => x.TransactionCategoryId);
        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => x.TransferGroupId);

        builder.HasIndex(x => new { x.BankAccountId, x.ExternalReference })
            .IsUnique()
            .HasFilter("[ExternalReference] IS NOT NULL");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.FinancialTransactions)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TransactionCategory)
            .WithMany(x => x.FinancialTransactions)
            .HasForeignKey(x => x.TransactionCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
