using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class GroupMember : BaseEntity
{
    public int UserId { get; private set; }
    public GroupMemberRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    // For EF Core
    private GroupMember() { }

    public GroupMember(int userId, GroupMemberRole role = GroupMemberRole.Member)
        : base()
    {
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    public void UpdateRole(GroupMemberRole role)
    {
        Role = role;
    }
}

public enum GroupMemberRole
{
    Owner,
    Admin,
    Member
}