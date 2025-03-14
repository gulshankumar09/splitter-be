using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ILogger<BudgetsController> _logger;

    public BudgetsController(IBudgetService budgetService, ILogger<BudgetsController> logger)
    {
        _budgetService = budgetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<BudgetDto>>>> GetAllBudgets()
    {
        var result = await _budgetService.GetAllBudgetsAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<Result<IEnumerable<BudgetDto>>>> GetActiveBudgets()
    {
        var result = await _budgetService.GetActiveBudgetsAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<BudgetDto>>> GetBudgetById(int id)
    {
        var result = await _budgetService.GetBudgetByIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<BudgetDto>>>> GetBudgetsByUserId(int userId)
    {
        var result = await _budgetService.GetBudgetsByUserIdAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<Result<IEnumerable<BudgetDto>>>> GetBudgetsByGroupId(int groupId)
    {
        var result = await _budgetService.GetBudgetsByGroupIdAsync(groupId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<BudgetDto>>> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        var result = await _budgetService.CreateBudgetAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetBudgetById), new { id = result.Data.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<BudgetDto>>> UpdateBudget(int id, [FromBody] UpdateBudgetRequest request)
    {
        var result = await _budgetService.UpdateBudgetAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<bool>>> DeleteBudget(int id)
    {
        var result = await _budgetService.DeleteBudgetAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<Result<bool>>> ActivateBudget(int id)
    {
        var result = await _budgetService.ActivateBudgetAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<Result<bool>>> DeactivateBudget(int id)
    {
        var result = await _budgetService.DeactivateBudgetAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/progress")]
    public async Task<ActionResult<Result<BudgetProgressDto>>> GetBudgetProgress(int id)
    {
        var result = await _budgetService.GetBudgetProgressAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/refresh")]
    public async Task<ActionResult<Result<BudgetDto>>> RefreshBudgetProgress(int id)
    {
        var result = await _budgetService.RefreshBudgetProgressAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Budget Category Management
    [HttpGet("{id}/categories")]
    public async Task<ActionResult<Result<IEnumerable<BudgetCategoryDto>>>> GetBudgetCategories(int id)
    {
        var result = await _budgetService.GetCategoriesByBudgetIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/categories")]
    public async Task<ActionResult<Result<BudgetDto>>> AddCategoryToBudget(int id, [FromBody] BudgetCategoryRequest request)
    {
        var result = await _budgetService.AddCategoryToBudgetAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/categories/{category}")]
    public async Task<ActionResult<Result<BudgetDto>>> UpdateCategoryLimit(int id, ExpenseCategory category, [FromBody] decimal? limitAmount)
    {
        var result = await _budgetService.UpdateCategoryLimitAsync(id, category, limitAmount);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}/categories/{category}")]
    public async Task<ActionResult<Result<bool>>> RemoveCategoryFromBudget(int id, ExpenseCategory category)
    {
        var result = await _budgetService.RemoveCategoryFromBudgetAsync(id, category);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/categories/{category}/progress")]
    public async Task<ActionResult<Result<BudgetCategoryDto>>> GetCategoryProgress(int id, ExpenseCategory category)
    {
        var result = await _budgetService.GetCategoryProgressAsync(id, category);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Budget Notifications
    [HttpPut("{id}/notifications")]
    public async Task<ActionResult<Result<BudgetDto>>> EnableBudgetNotifications(int id, [FromBody] bool enabled)
    {
        var result = await _budgetService.EnableBudgetNotificationsAsync(id, enabled);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/threshold")]
    public async Task<ActionResult<Result<BudgetDto>>> SetBudgetThreshold(int id, [FromBody] BudgetNotificationThreshold threshold)
    {
        var result = await _budgetService.SetBudgetNotificationThresholdAsync(id, threshold);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/alerts")]
    public async Task<ActionResult<Result<IEnumerable<BudgetAlertDto>>>> GetBudgetAlerts(int id)
    {
        var result = await _budgetService.GetAlertsByBudgetIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("alerts/user/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<BudgetAlertDto>>>> GetUnreadAlertsByUserId(int userId)
    {
        var result = await _budgetService.GetUnreadAlertsByUserIdAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("alerts/{alertId}/mark-read")]
    public async Task<ActionResult<Result<bool>>> MarkAlertAsRead(int alertId)
    {
        var result = await _budgetService.MarkAlertAsReadAsync(alertId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Budget Processing (typically called by background service)
    [HttpPost("process")]
    public async Task<ActionResult<Result<bool>>> ProcessBudgets()
    {
        var result = await _budgetService.ProcessBudgetsAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("check-thresholds")]
    public async Task<ActionResult<Result<bool>>> CheckBudgetThresholds([FromQuery] int? budgetId = null)
    {
        var result = await _budgetService.CheckBudgetThresholdsAsync(budgetId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("renew-recurring")]
    public async Task<ActionResult<Result<bool>>> RenewRecurringBudgets()
    {
        var result = await _budgetService.RenewRecurringBudgetsAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}