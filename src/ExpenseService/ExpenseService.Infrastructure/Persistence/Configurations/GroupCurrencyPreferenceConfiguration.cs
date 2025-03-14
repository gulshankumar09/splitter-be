using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class GroupCurrencyPreferenceConfiguration : IEntityTypeConfiguration<GroupCurrencyPreference>
{
    public void Configure(EntityTypeBuilder<GroupCurrencyPreference> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.GroupId)
            .IsRequired();

        builder.Property(g => g.DefaultCurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(g => g.AutoConvertForMembers)
            .IsRequired();

        // Ensure one preference per group
        builder.HasIndex(g => g.GroupId)
            .IsUnique();
    }
}