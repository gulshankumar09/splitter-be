using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalancesController : ControllerBase
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<BalancesController> _logger;

    public BalancesController(IBalanceService balanceService, ILogger<BalancesController> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    [HttpGet("global")]
    public async Task<ActionResult<Result<GlobalBalanceSummaryDto>>> GetGlobalBalanceSummary()
    {
        var result = await _balanceService.GetGlobalBalanceSummaryAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("user")]
    public async Task<ActionResult<Result<GlobalBalanceSummaryDto>>> GetUserBalanceSummary(
        [FromQuery] UserBalanceQueryDto query)
    {
        var result = await _balanceService.GetUserBalanceSummaryAsync(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("settlement-plan/{userId}")]
    public async Task<ActionResult<Result<List<SettlementTransactionDto>>>> GetSettlementPlan(int userId)
    {
        var result = await _balanceService.GetSettlementPlanAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("settlements")]
    public async Task<ActionResult<Result<SettlementTransactionDto>>> RecordSettlement(
        [FromBody] RecordSettlementRequest request)
    {
        var result = await _balanceService.RecordSettlementAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("settlements/{settlementId}/complete")]
    public async Task<ActionResult<Result<SettlementTransactionDto>>> MarkSettlementCompleted(int settlementId)
    {
        var result = await _balanceService.MarkSettlementCompletedAsync(settlementId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("settlements/{settlementId}/cancel")]
    public async Task<ActionResult<Result<SettlementTransactionDto>>> CancelSettlement(int settlementId)
    {
        var result = await _balanceService.CancelSettlementAsync(settlementId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}