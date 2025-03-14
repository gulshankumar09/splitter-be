using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IGroupService
{
    Task<Result<GroupDto>> GetGroupByIdAsync(int id);
    Task<Result<IEnumerable<GroupDto>>> GetAllGroupsAsync();
    Task<Result<IEnumerable<GroupDto>>> GetGroupsByUserIdAsync(int userId);
    Task<Result<IEnumerable<GroupDto>>> GetGroupsByTypeAsync(GroupType type);
    Task<Result<GroupDto>> CreateGroupAsync(CreateGroupRequest request);
    Task<Result<GroupDto>> UpdateGroupAsync(int id, UpdateGroupRequest request);
    Task<Result> DeleteGroupAsync(int id);
    Task<Result<GroupDto>> AddMemberAsync(int groupId, AddMemberRequest request);
    Task<Result<GroupDto>> RemoveMemberAsync(int groupId, int userId);
    Task<Result<GroupDto>> UpdateMemberRoleAsync(int groupId, UpdateMemberRoleRequest request);
    Task<Result<GroupBalanceSummaryDto>> GetBalanceSummaryAsync(int groupId);
    Task<Result<GroupDto>> AddExpenseToGroupAsync(int groupId, int expenseId);
    Task<Result<GroupDto>> RemoveExpenseFromGroupAsync(int groupId, int expenseId);
}