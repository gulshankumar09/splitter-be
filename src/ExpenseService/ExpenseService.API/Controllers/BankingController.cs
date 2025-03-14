using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankingController : ControllerBase
{
    private readonly IBankingIntegrationService _bankingService;
    private readonly ILogger<BankingController> _logger;

    public BankingController(IBankingIntegrationService bankingService, ILogger<BankingController> logger)
    {
        _bankingService = bankingService;
        _logger = logger;
    }

    [HttpGet("providers")]
    public async Task<ActionResult<Result<IEnumerable<BankingProviderDto>>>> GetBankingProviders()
    {
        var result = await _bankingService.GetAvailableBankingProvidersAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("accounts/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<BankAccountDto>>>> GetConnectedAccounts(int userId)
    {
        var result = await _bankingService.GetConnectedAccountsAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("connect")]
    public async Task<ActionResult<Result<BankAccountDto>>> ConnectBankAccount([FromBody] ConnectBankAccountRequest request)
    {
        var result = await _bankingService.ConnectBankAccountAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetConnectedAccounts), new { userId = request.UserId }, result)
            : BadRequest(result);
    }

    [HttpPost("disconnect/{accountId}")]
    public async Task<ActionResult<Result<bool>>> DisconnectBankAccount(int accountId)
    {
        var result = await _bankingService.DisconnectBankAccountAsync(accountId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("import")]
    public async Task<ActionResult<Result<ImportTransactionsResultDto>>> ImportTransactions([FromBody] ImportTransactionsRequest request)
    {
        var result = await _bankingService.ImportTransactionsAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sync/{accountId}")]
    public async Task<ActionResult<Result<bool>>> SyncTransactions(int accountId)
    {
        var result = await _bankingService.SyncTransactionStatusAsync(accountId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}