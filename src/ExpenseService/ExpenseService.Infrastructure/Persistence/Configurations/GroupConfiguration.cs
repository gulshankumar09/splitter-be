using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedLibrary.Infrastructure.Configurations;

namespace ExpenseService.Infrastructure.Persistence.Configurations;

public class GroupConfiguration : BaseEntityConfiguration<Group>
{
    public override void Configure(EntityTypeBuilder<Group> builder)
    {
        base.Configure(builder);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.Property(g => g.OwnerId)
            .IsRequired();

        builder.Property(g => g.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(g => g.ImageUrl)
            .HasMaxLength(1024);

        builder.HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey("GroupId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Expenses)
            .WithOne()
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class GroupMemberConfiguration : BaseEntityConfiguration<GroupMember>
{
    public override void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        base.Configure(builder);

        builder.Property(gm => gm.UserId)
            .IsRequired();

        builder.Property(gm => gm.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(gm => gm.JoinedAt)
            .IsRequired();

        // Create a unique index on GroupId and UserId
        builder.HasIndex("GroupId", nameof(GroupMember.UserId))
            .IsUnique();
    }
}