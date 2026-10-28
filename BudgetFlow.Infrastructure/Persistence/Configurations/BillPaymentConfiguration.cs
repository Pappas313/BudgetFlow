using BudgetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetFlow.Infrastructure.Persistence.Configurations;

public class BillPaymentConfiguration : IEntityTypeConfiguration<BillPayment>
{
    public void Configure(EntityTypeBuilder<BillPayment> builder)
    {
        builder.ToTable("BillPayments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmountPaid)
            .HasColumnType("decimal(19,4)")
            .IsRequired();

        builder.Property(x => x.PaidAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.BillId);

        builder.HasQueryFilter(x => x.Bill == null || !x.Bill.IsDeleted);

        builder.HasOne(x => x.Bill)
            .WithMany(x => x.BillPayments)
            .HasForeignKey(x => x.BillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
