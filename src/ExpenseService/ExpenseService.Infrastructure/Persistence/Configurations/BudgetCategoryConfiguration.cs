using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
{
    public void Configure(EntityTypeBuilder<BudgetCategory> builder)
    {
        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(bc => bc.LimitAmount)
            .HasPrecision(18, 2);

        // Create index for budget id and category for unique checking
        builder.HasIndex("BudgetId", nameof(BudgetCategory.Category))
            .IsUnique();
    }
}