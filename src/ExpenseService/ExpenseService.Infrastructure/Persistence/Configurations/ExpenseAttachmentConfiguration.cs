using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class ExpenseAttachmentConfiguration : BaseEntityConfiguration<ExpenseAttachment>
{
    public override void Configure(EntityTypeBuilder<ExpenseAttachment> builder)
    {
        base.Configure(builder);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FileType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FileUrl)
            .IsRequired()
            .HasMaxLength(1024);
    }
}