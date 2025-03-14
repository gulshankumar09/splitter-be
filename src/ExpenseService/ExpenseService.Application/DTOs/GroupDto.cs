using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public GroupType Type { get; set; }
    public string? ImageUrl { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GroupMemberDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public GroupMemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public GroupType Type { get; set; }
    public string? ImageUrl { get; set; }
    public List<int>? InitialMemberIds { get; set; }
}

public class UpdateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GroupType Type { get; set; }
    public string? ImageUrl { get; set; }
}

public class AddMemberRequest
{
    public int UserId { get; set; }
    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;
}

public class UpdateMemberRoleRequest
{
    public int UserId { get; set; }
    public GroupMemberRole Role { get; set; }
}

public class GroupBalanceSummaryDto
{
    public int GroupId { get; set; }
    public List<MemberBalanceDto> MemberBalances { get; set; } = new();
    public List<BalanceTransactionDto> SettlementTransactions { get; set; } = new();
}

public class MemberBalanceDto
{
    public int UserId { get; set; }
    public decimal Balance { get; set; }
}

public class BalanceTransactionDto
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public decimal Amount { get; set; }
}