using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class SettlementRecordConfiguration : BaseEntityConfiguration<SettlementRecord>
{
    public override void Configure(EntityTypeBuilder<SettlementRecord> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.FromUserId)
            .IsRequired();

        builder.Property(s => s.ToUserId)
            .IsRequired();

        builder.Property(s => s.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Notes)
            .HasMaxLength(500);
    }
}