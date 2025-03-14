using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class BudgetAlertConfiguration : IEntityTypeConfiguration<BudgetAlert>
{
    public void Configure(EntityTypeBuilder<BudgetAlert> builder)
    {
        builder.HasKey(ba => ba.Id);

        builder.Property(ba => ba.BudgetId)
            .IsRequired();

        builder.Property(ba => ba.UserId)
            .IsRequired();

        builder.Property(ba => ba.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ba => ba.ThresholdPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(ba => ba.CurrentSpending)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ba => ba.BudgetLimit)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ba => ba.IsRead)
            .IsRequired();

        builder.Property(ba => ba.ReadAt);

        // Create indexes for faster lookups
        builder.HasIndex(ba => ba.BudgetId);
        builder.HasIndex(ba => ba.UserId);
        builder.HasIndex(ba => ba.IsRead);
    }
}