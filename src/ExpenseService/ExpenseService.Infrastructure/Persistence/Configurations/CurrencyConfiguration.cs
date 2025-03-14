using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Symbol)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.IsDefault)
            .IsRequired();

        // Enforce unique currency code
        builder.HasIndex(c => c.Code)
            .IsUnique();

        // Ensure only one default currency
        builder.HasIndex(c => c.IsDefault)
            .HasFilter("[IsDefault] = 1")
            .IsUnique();
    }
}