using ExpenseService.Application.DTOs;
using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IReportingService
{
    Task<Result<ExpenseSummaryDto>> GetExpenseSummaryAsync(ExpenseReportRequestDto request);
    Task<Result<List<CategoryBreakdownDto>>> GetCategoryBreakdownAsync(ExpenseReportRequestDto request);
    Task<Result<SpendingTrendDto>> GetSpendingTrendAsync(ExpenseReportRequestDto request);
    Task<Result<byte[]>> ExportReportAsync(ExpenseReportRequestDto request);
}