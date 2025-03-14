using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FromCurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.ToCurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Rate)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(e => e.EffectiveDate)
            .IsRequired();

        builder.Property(e => e.Source)
            .HasMaxLength(50);

        // Create a composite index for currency pair and effective date
        builder.HasIndex(e => new { e.FromCurrencyCode, e.ToCurrencyCode, e.EffectiveDate })
            .IsUnique();

        // Create indexes for faster lookup by currency codes
        builder.HasIndex(e => e.FromCurrencyCode);
        builder.HasIndex(e => e.ToCurrencyCode);
    }
}