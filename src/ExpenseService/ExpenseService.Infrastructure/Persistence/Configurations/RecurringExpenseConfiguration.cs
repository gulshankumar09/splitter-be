using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;
using System.Text.Json;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class RecurringExpenseConfiguration : BaseEntityConfiguration<RecurringExpense>
{
    public override void Configure(EntityTypeBuilder<RecurringExpense> builder)
    {
        base.Configure(builder);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.CreatedByUserId)
            .IsRequired();

        builder.Property(r => r.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Frequency)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.FrequencyParam);

        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.EndDate);

        builder.Property(r => r.NextOccurrence)
            .IsRequired();

        builder.Property(r => r.LastProcessed);

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.GroupId);

        builder.Property(r => r.SplitType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.SendReminders)
            .IsRequired();

        builder.Property(r => r.ReminderDaysBefore)
            .IsRequired();

        // Store ParticipantUserIds as JSON array
        builder.Property(r => r.ParticipantUserIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<List<int>>(v, new JsonSerializerOptions()) ?? new List<int>());
    }
}