using ExpenseService.Application.DTOs;
using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IBalanceService
{
    Task<Result<GlobalBalanceSummaryDto>> GetGlobalBalanceSummaryAsync();
    Task<Result<GlobalBalanceSummaryDto>> GetUserBalanceSummaryAsync(UserBalanceQueryDto query);
    Task<Result<List<SettlementTransactionDto>>> GetSettlementPlanAsync(int userId);
    Task<Result<SettlementTransactionDto>> RecordSettlementAsync(RecordSettlementRequest request);
    Task<Result<SettlementTransactionDto>> MarkSettlementCompletedAsync(int settlementId);
    Task<Result<SettlementTransactionDto>> CancelSettlementAsync(int settlementId);
}