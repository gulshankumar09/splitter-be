using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(IExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<ExpenseDto>>>> GetAllExpenses()
    {
        var result = await _expenseService.GetAllExpensesAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<ExpenseDto>>> GetExpenseById(int id)
    {
        var result = await _expenseService.GetExpenseByIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<ExpenseDto>>>> GetExpensesByUserId(int userId)
    {
        var result = await _expenseService.GetExpensesByUserIdAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<Result<IEnumerable<ExpenseDto>>>> GetExpensesByCategory(ExpenseCategory category)
    {
        var result = await _expenseService.GetExpensesByCategoryAsync(category);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tag/{tag}")]
    public async Task<ActionResult<Result<IEnumerable<ExpenseDto>>>> GetExpensesByTag(string tag)
    {
        var result = await _expenseService.GetExpensesByTagAsync(tag);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<Result<IEnumerable<ExpenseDto>>>> GetExpensesByDateRange(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _expenseService.GetExpensesByDateRangeAsync(startDate, endDate);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<ExpenseDto>>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        var result = await _expenseService.CreateExpenseAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetExpenseById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<ExpenseDto>>> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        var result = await _expenseService.UpdateExpenseAsync(id, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteExpense(int id)
    {
        var result = await _expenseService.DeleteExpenseAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{expenseId}/attachments")]
    public async Task<ActionResult<Result<ExpenseDto>>> AddAttachment(
        int expenseId, [FromBody] AddAttachmentRequest request)
    {
        var result = await _expenseService.AddAttachmentAsync(expenseId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{expenseId}/attachments/{attachmentId}")]
    public async Task<ActionResult<Result<ExpenseDto>>> RemoveAttachment(int expenseId, int attachmentId)
    {
        var result = await _expenseService.RemoveAttachmentAsync(expenseId, attachmentId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{expenseId}/splits/mark-paid")]
    public async Task<ActionResult<Result<ExpenseDto>>> MarkSplitAsPaid(
        int expenseId, [FromBody] MarkSplitPaidRequest request)
    {
        var result = await _expenseService.MarkSplitAsPaidAsync(expenseId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{expenseId}/splits/mark-unpaid")]
    public async Task<ActionResult<Result<ExpenseDto>>> MarkSplitAsUnpaid(
        int expenseId, [FromBody] MarkSplitPaidRequest request)
    {
        var result = await _expenseService.MarkSplitAsUnpaidAsync(expenseId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}