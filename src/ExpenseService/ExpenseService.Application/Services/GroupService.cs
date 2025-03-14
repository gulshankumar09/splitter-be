using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IExpenseRepository _expenseRepository;

    public GroupService(IGroupRepository groupRepository, IExpenseRepository expenseRepository)
    {
        _groupRepository = groupRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<GroupDto>> GetGroupByIdAsync(int id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Result<GroupDto>.Failure("Group not found");
        }

        return Result<GroupDto>.Success(MapToDto(group));
    }

    public async Task<Result<IEnumerable<GroupDto>>> GetAllGroupsAsync()
    {
        var groups = await _groupRepository.GetAllAsync();
        var groupDtos = groups.Select(MapToDto);
        return Result<IEnumerable<GroupDto>>.Success(groupDtos);
    }

    public async Task<Result<IEnumerable<GroupDto>>> GetGroupsByUserIdAsync(int userId)
    {
        var groups = await _groupRepository.GetByUserIdAsync(userId);
        var groupDtos = groups.Select(MapToDto);
        return Result<IEnumerable<GroupDto>>.Success(groupDtos);
    }

    public async Task<Result<IEnumerable<GroupDto>>> GetGroupsByTypeAsync(GroupType type)
    {
        var groups = await _groupRepository.GetByTypeAsync(type);
        var groupDtos = groups.Select(MapToDto);
        return Result<IEnumerable<GroupDto>>.Success(groupDtos);
    }

    public async Task<Result<GroupDto>> CreateGroupAsync(CreateGroupRequest request)
    {
        try
        {
            var group = new Group(
                request.Name,
                request.OwnerId,
                request.Type,
                request.Description);

            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                group.SetImageUrl(request.ImageUrl);
            }

            if (request.InitialMemberIds != null)
            {
                foreach (var memberId in request.InitialMemberIds)
                {
                    if (memberId != request.OwnerId) // Owner is already added automatically
                    {
                        group.AddMember(memberId);
                    }
                }
            }

            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to create group: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> UpdateGroupAsync(int id, UpdateGroupRequest request)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(id);
            if (group == null)
            {
                return Result<GroupDto>.Failure("Group not found");
            }

            group.UpdateDetails(
                request.Name,
                request.Type,
                request.Description);

            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                group.SetImageUrl(request.ImageUrl);
            }

            _groupRepository.Update(group);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to update group: {ex.Message}");
        }
    }

    public async Task<Result> DeleteGroupAsync(int id)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(id);
            if (group == null)
            {
                return Result.Failure("Group not found");
            }

            _groupRepository.Delete(group);
            await _groupRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete group: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> AddMemberAsync(int groupId, AddMemberRequest request)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return Result<GroupDto>.Failure("Group not found");
            }

            group.AddMember(request.UserId, request.Role);

            _groupRepository.Update(group);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to add member: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> RemoveMemberAsync(int groupId, int userId)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return Result<GroupDto>.Failure("Group not found");
            }

            try
            {
                group.RemoveMember(userId);
            }
            catch (InvalidOperationException ex)
            {
                return Result<GroupDto>.Failure(ex.Message);
            }

            _groupRepository.Update(group);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to remove member: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> UpdateMemberRoleAsync(int groupId, UpdateMemberRoleRequest request)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return Result<GroupDto>.Failure("Group not found");
            }

            group.UpdateMemberRole(request.UserId, request.Role);

            _groupRepository.Update(group);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to update member role: {ex.Message}");
        }
    }

    public async Task<Result<GroupBalanceSummaryDto>> GetBalanceSummaryAsync(int groupId)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return Result<GroupBalanceSummaryDto>.Failure("Group not found");
            }

            var balanceSummary = group.GenerateBalanceSummary();

            return Result<GroupBalanceSummaryDto>.Success(new GroupBalanceSummaryDto
            {
                GroupId = balanceSummary.GroupId,
                MemberBalances = balanceSummary.MemberBalances.Select(mb => new MemberBalanceDto
                {
                    UserId = mb.UserId,
                    Balance = mb.Balance
                }).ToList(),
                SettlementTransactions = balanceSummary.SettlementTransactions.Select(bt => new BalanceTransactionDto
                {
                    FromUserId = bt.FromUserId,
                    ToUserId = bt.ToUserId,
                    Amount = bt.Amount
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return Result<GroupBalanceSummaryDto>.Failure($"Failed to generate balance summary: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> AddExpenseToGroupAsync(int groupId, int expenseId)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return Result<GroupDto>.Failure("Group not found");
            }

            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<GroupDto>.Failure("Expense not found");
            }

            expense.AssignToGroup(groupId);
            group.AddExpense(expense);

            _expenseRepository.Update(expense);
            _groupRepository.Update(group);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to add expense to group: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> RemoveExpenseFromGroupAsync(int groupId, int expenseId)
    {
        try
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return Result<GroupDto>.Failure("Group not found");
            }

            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<GroupDto>.Failure("Expense not found");
            }

            expense.RemoveFromGroup();

            _expenseRepository.Update(expense);
            await _groupRepository.SaveChangesAsync();

            return Result<GroupDto>.Success(MapToDto(group));
        }
        catch (Exception ex)
        {
            return Result<GroupDto>.Failure($"Failed to remove expense from group: {ex.Message}");
        }
    }

    private static GroupDto MapToDto(Group group)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            OwnerId = group.OwnerId,
            Type = group.Type,
            ImageUrl = group.ImageUrl,
            Members = group.Members.Select(m => new GroupMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList(),
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        };
    }
}