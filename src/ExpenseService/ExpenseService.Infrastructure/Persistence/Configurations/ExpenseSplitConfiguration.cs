using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class ExpenseSplitConfiguration : BaseEntityConfiguration<ExpenseSplit>
{
    public override void Configure(EntityTypeBuilder<ExpenseSplit> builder)
    {
        base.Configure(builder);

        builder.Property(es => es.UserId)
            .IsRequired();

        builder.Property(es => es.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(es => es.SplitType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(es => es.Percentage)
            .HasPrecision(5, 2);

        builder.Property(es => es.IsPaid)
            .IsRequired();

        builder.Property(es => es.PaidDate);
    }
}