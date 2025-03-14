using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<GroupDto>>>> GetAllGroups()
    {
        var result = await _groupService.GetAllGroupsAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<GroupDto>>> GetGroupById(int id)
    {
        var result = await _groupService.GetGroupByIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<GroupDto>>>> GetGroupsByUserId(int userId)
    {
        var result = await _groupService.GetGroupsByUserIdAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<Result<IEnumerable<GroupDto>>>> GetGroupsByType(GroupType type)
    {
        var result = await _groupService.GetGroupsByTypeAsync(type);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<GroupDto>>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var result = await _groupService.CreateGroupAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetGroupById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<GroupDto>>> UpdateGroup(int id, [FromBody] UpdateGroupRequest request)
    {
        var result = await _groupService.UpdateGroupAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteGroup(int id)
    {
        var result = await _groupService.DeleteGroupAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{groupId}/members")]
    public async Task<ActionResult<Result<GroupDto>>> AddMember(int groupId, [FromBody] AddMemberRequest request)
    {
        var result = await _groupService.AddMemberAsync(groupId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{groupId}/members/{userId}")]
    public async Task<ActionResult<Result<GroupDto>>> RemoveMember(int groupId, int userId)
    {
        var result = await _groupService.RemoveMemberAsync(groupId, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{groupId}/members/role")]
    public async Task<ActionResult<Result<GroupDto>>> UpdateMemberRole(
        int groupId, [FromBody] UpdateMemberRoleRequest request)
    {
        var result = await _groupService.UpdateMemberRoleAsync(groupId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{groupId}/balance-summary")]
    public async Task<ActionResult<Result<GroupBalanceSummaryDto>>> GetBalanceSummary(int groupId)
    {
        var result = await _groupService.GetBalanceSummaryAsync(groupId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{groupId}/expenses/{expenseId}")]
    public async Task<ActionResult<Result<GroupDto>>> AddExpenseToGroup(int groupId, int expenseId)
    {
        var result = await _groupService.AddExpenseToGroupAsync(groupId, expenseId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{groupId}/expenses/{expenseId}")]
    public async Task<ActionResult<Result<GroupDto>>> RemoveExpenseFromGroup(int groupId, int expenseId)
    {
        var result = await _groupService.RemoveExpenseFromGroupAsync(groupId, expenseId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}