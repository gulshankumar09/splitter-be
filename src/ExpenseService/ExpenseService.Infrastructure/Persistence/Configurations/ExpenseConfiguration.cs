using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;
using System.Text.Json;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : BaseEntityConfiguration<Expense>
{
    public override void Configure(EntityTypeBuilder<Expense> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.PaidByUserId)
            .IsRequired();

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.DefaultSplitType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.ExpenseDate)
            .IsRequired();

        // Store Tags as JSON array
        builder.Property(e => e.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>());

        builder.Property(e => e.GroupId)
            .IsRequired(false);

        builder.HasMany(e => e.Splits)
            .WithOne()
            .HasForeignKey("ExpenseId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Attachments)
            .WithOne()
            .HasForeignKey("ExpenseId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}