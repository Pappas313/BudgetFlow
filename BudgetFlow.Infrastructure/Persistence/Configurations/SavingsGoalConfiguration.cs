using BudgetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetFlow.Infrastructure.Persistence.Configurations;

public class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
{
    public void Configure(EntityTypeBuilder<SavingsGoal> builder)
    {
        builder.ToTable("SavingsGoals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TargetAmount)
            .HasColumnType("decimal(19,4)")
            .IsRequired();

        builder.Property(x => x.CurrentAmount)
            .HasColumnType("decimal(19,4)")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.ApplicationUserId, x.Name })
            .IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.SavingsContributions)
            .WithOne(x => x.SavingsGoal)
            .HasForeignKey(x => x.SavingsGoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
