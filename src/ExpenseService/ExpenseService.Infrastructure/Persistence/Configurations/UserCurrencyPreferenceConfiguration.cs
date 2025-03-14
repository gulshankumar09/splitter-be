using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class UserCurrencyPreferenceConfiguration : IEntityTypeConfiguration<UserCurrencyPreference>
{
    public void Configure(EntityTypeBuilder<UserCurrencyPreference> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.DefaultCurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(u => u.AutoConvert)
            .IsRequired();

        // Ensure one preference per user
        builder.HasIndex(u => u.UserId)
            .IsUnique();
    }
}