using BudgetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetFlow.Infrastructure.Persistence.Configurations;

public class SavingsContributionConfiguration : IEntityTypeConfiguration<SavingsContribution>
{
    public void Configure(EntityTypeBuilder<SavingsContribution> builder)
    {
        builder.ToTable("SavingsContributions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(19,4)")
            .IsRequired();

        builder.Property(x => x.ContributedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.SavingsGoalId);

        builder.HasQueryFilter(x => x.SavingsGoal == null || !x.SavingsGoal.IsDeleted);

        builder.HasOne(x => x.SavingsGoal)
            .WithMany(x => x.SavingsContributions)
            .HasForeignKey(x => x.SavingsGoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
