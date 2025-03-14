using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.UserId)
            .IsRequired();

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(b => b.Period)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(b => b.StartDate)
            .IsRequired();

        builder.Property(b => b.EndDate)
            .IsRequired();

        builder.Property(b => b.IsRecurring)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired();

        builder.Property(b => b.WarningThreshold)
            .HasConversion<string>();

        builder.Property(b => b.NotificationsEnabled)
            .IsRequired();

        // Create indexes for faster lookups
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.GroupId);
        builder.HasIndex(b => b.Period);
        builder.HasIndex(b => b.IsActive);

        // Define relationships
        builder.HasMany(b => b.Categories)
            .WithOne()
            .HasForeignKey("BudgetId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}