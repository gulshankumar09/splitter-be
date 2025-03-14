using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecurringExpensesController : ControllerBase
{
    private readonly IRecurringExpenseService _recurringExpenseService;
    private readonly ILogger<RecurringExpensesController> _logger;

    public RecurringExpensesController(
        IRecurringExpenseService recurringExpenseService,
        ILogger<RecurringExpensesController> logger)
    {
        _recurringExpenseService = recurringExpenseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<RecurringExpenseDto>>>> GetAllRecurringExpenses()
    {
        var result = await _recurringExpenseService.GetAllRecurringExpensesAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<RecurringExpenseDto>>> GetRecurringExpenseById(int id)
    {
        var result = await _recurringExpenseService.GetRecurringExpenseByIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<RecurringExpenseDto>>>> GetRecurringExpensesByUserId(int userId)
    {
        var result = await _recurringExpenseService.GetRecurringExpensesByUserIdAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<Result<IEnumerable<RecurringExpenseDto>>>> GetRecurringExpensesByGroupId(int groupId)
    {
        var result = await _recurringExpenseService.GetRecurringExpensesByGroupIdAsync(groupId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<RecurringExpenseDto>>> CreateRecurringExpense(
        [FromBody] CreateRecurringExpenseRequest request)
    {
        var result = await _recurringExpenseService.CreateRecurringExpenseAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetRecurringExpenseById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<RecurringExpenseDto>>> UpdateRecurringExpense(
        int id, [FromBody] UpdateRecurringExpenseRequest request)
    {
        var result = await _recurringExpenseService.UpdateRecurringExpenseAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/notifications")]
    public async Task<ActionResult<Result<RecurringExpenseDto>>> UpdateNotificationSettings(
        int id, [FromBody] UpdateNotificationSettingsRequest request)
    {
        var result = await _recurringExpenseService.UpdateNotificationSettingsAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/activate")]
    public async Task<ActionResult<Result<RecurringExpenseDto>>> ActivateRecurringExpense(int id)
    {
        var result = await _recurringExpenseService.ActivateRecurringExpenseAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/deactivate")]
    public async Task<ActionResult<Result<RecurringExpenseDto>>> DeactivateRecurringExpense(int id)
    {
        var result = await _recurringExpenseService.DeactivateRecurringExpenseAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteRecurringExpense(int id)
    {
        var result = await _recurringExpenseService.DeleteRecurringExpenseAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("reminders/{userId}")]
    public async Task<ActionResult<Result<List<RecurringExpenseReminderDto>>>> GetUpcomingReminders(int userId)
    {
        var result = await _recurringExpenseService.GetUpcomingExpenseRemindersAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("process")]
    public async Task<ActionResult<Result>> ProcessDueRecurringExpenses()
    {
        var result = await _recurringExpenseService.ProcessDueRecurringExpensesAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}