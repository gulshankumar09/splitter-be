using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class ReportingService : IReportingService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExportService _exportService;

    public ReportingService(
        IExpenseRepository expenseRepository,
        IExportService exportService)
    {
        _expenseRepository = expenseRepository;
        _exportService = exportService;
    }

    public async Task<Result<ExpenseSummaryDto>> GetExpenseSummaryAsync(ExpenseReportRequestDto request)
    {
        try
        {
            // Get expenses based on filters
            var expenses = await GetFilteredExpensesAsync(request);
            if (!expenses.Any())
            {
                return Result<ExpenseSummaryDto>.Success(new ExpenseSummaryDto
                {
                    Period = request.Period,
                    TotalAmount = 0,
                    TotalExpenses = 0,
                    AverageExpenseAmount = 0
                });
            }

            // Calculate summary statistics
            var totalAmount = expenses.Sum(e => e.Amount);
            var totalExpenses = expenses.Count();
            var averageAmount = totalAmount / totalExpenses;

            // Calculate category breakdown
            var categoryBreakdown = GetCategoryBreakdown(expenses);

            // Calculate monthly breakdown
            var monthlyBreakdown = GetMonthlyBreakdown(expenses);

            var summary = new ExpenseSummaryDto
            {
                Period = request.Period,
                TotalAmount = totalAmount,
                TotalExpenses = totalExpenses,
                AverageExpenseAmount = averageAmount,
                CategoryBreakdown = categoryBreakdown,
                MonthlyBreakdown = monthlyBreakdown
            };

            return Result<ExpenseSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<ExpenseSummaryDto>.Failure($"Failed to generate expense summary: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryBreakdownDto>>> GetCategoryBreakdownAsync(ExpenseReportRequestDto request)
    {
        try
        {
            // Get expenses based on filters
            var expenses = await GetFilteredExpensesAsync(request);
            if (!expenses.Any())
            {
                return Result<List<CategoryBreakdownDto>>.Success(new List<CategoryBreakdownDto>());
            }

            // Calculate category breakdown
            var categoryBreakdown = GetCategoryBreakdown(expenses);

            return Result<List<CategoryBreakdownDto>>.Success(categoryBreakdown);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryBreakdownDto>>.Failure($"Failed to generate category breakdown: {ex.Message}");
        }
    }

    public async Task<Result<SpendingTrendDto>> GetSpendingTrendAsync(ExpenseReportRequestDto request)
    {
        try
        {
            // Get expenses based on filters
            var expenses = await GetFilteredExpensesAsync(request);
            if (!expenses.Any())
            {
                return Result<SpendingTrendDto>.Success(new SpendingTrendDto
                {
                    TrendPoints = new List<TrendPointDto>(),
                    Statistics = new TrendStatisticsDto()
                });
            }

            // Group expenses based on the aggregation period
            var groupedExpenses = GroupExpensesByPeriod(expenses, request.AggregationPeriod);

            // Calculate trend points
            var trendPoints = groupedExpenses.Select(g => new TrendPointDto
            {
                Date = g.Key,
                Amount = g.Sum(e => e.Amount),
                ExpenseCount = g.Count()
            }).OrderBy(tp => tp.Date).ToList();

            // Calculate trend statistics
            var statistics = CalculateTrendStatistics(trendPoints);

            var trend = new SpendingTrendDto
            {
                TrendPoints = trendPoints,
                Statistics = statistics
            };

            return Result<SpendingTrendDto>.Success(trend);
        }
        catch (Exception ex)
        {
            return Result<SpendingTrendDto>.Failure($"Failed to generate spending trend: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> ExportReportAsync(ExpenseReportRequestDto request)
    {
        try
        {
            // Determine what to export based on the report type
            switch (request.ReportType)
            {
                case ReportType.Summary:
                    var summaryResult = await GetExpenseSummaryAsync(request);
                    if (!summaryResult.IsSuccess)
                    {
                        return Result<byte[]>.Failure(summaryResult.Error);
                    }
                    return await ExportDataAsync(summaryResult.Data, request.ExportFormat);

                case ReportType.CategoryBreakdown:
                    var breakdownResult = await GetCategoryBreakdownAsync(request);
                    if (!breakdownResult.IsSuccess)
                    {
                        return Result<byte[]>.Failure(breakdownResult.Error);
                    }
                    return await ExportDataAsync(breakdownResult.Data, request.ExportFormat);

                case ReportType.SpendingTrend:
                    var trendResult = await GetSpendingTrendAsync(request);
                    if (!trendResult.IsSuccess)
                    {
                        return Result<byte[]>.Failure(trendResult.Error);
                    }
                    return await ExportDataAsync(trendResult.Data.TrendPoints, request.ExportFormat);

                default:
                    return Result<byte[]>.Failure("Invalid report type");
            }
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Failed to export report: {ex.Message}");
        }
    }

    #region Helper Methods

    private async Task<IEnumerable<Expense>> GetFilteredExpensesAsync(ExpenseReportRequestDto request)
    {
        IEnumerable<Expense> expenses;

        // Get expenses for the specified date range
        expenses = await _expenseRepository.GetByDateRangeAsync(request.Period.StartDate, request.Period.EndDate);

        // Apply user filter if specified
        if (request.UserId.HasValue)
        {
            expenses = expenses.Where(e =>
                e.PaidByUserId == request.UserId.Value ||
                e.Splits.Any(s => s.UserId == request.UserId.Value));
        }

        // Apply group filter if specified
        if (request.GroupId.HasValue)
        {
            expenses = expenses.Where(e => e.GroupId == request.GroupId.Value);
        }

        // Apply category filter if specified
        if (request.Categories != null && request.Categories.Any())
        {
            expenses = expenses.Where(e => request.Categories.Contains(e.Category));
        }

        // Apply amount filters if specified
        if (request.MinAmount.HasValue)
        {
            expenses = expenses.Where(e => e.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            expenses = expenses.Where(e => e.Amount <= request.MaxAmount.Value);
        }

        return expenses;
    }

    private List<CategoryBreakdownDto> GetCategoryBreakdown(IEnumerable<Expense> expenses)
    {
        var totalAmount = expenses.Sum(e => e.Amount);

        var breakdown = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategoryBreakdownDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount),
                ExpenseCount = g.Count(),
                Percentage = g.Sum(e => e.Amount) / totalAmount * 100
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        return breakdown;
    }

    private List<MonthlyBreakdownDto> GetMonthlyBreakdown(IEnumerable<Expense> expenses)
    {
        var breakdown = expenses
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new MonthlyBreakdownDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                TotalAmount = g.Sum(e => e.Amount),
                ExpenseCount = g.Count(),
                CategoryBreakdown = GetCategoryBreakdown(g)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return breakdown;
    }

    private IEnumerable<IGrouping<DateTime, Expense>> GroupExpensesByPeriod(
        IEnumerable<Expense> expenses, AggregationPeriod period)
    {
        switch (period)
        {
            case AggregationPeriod.Daily:
                return expenses.GroupBy(e => e.ExpenseDate.Date);

            case AggregationPeriod.Weekly:
                return expenses.GroupBy(e =>
                {
                    // Calculate the first day of the week for the expense date
                    var diff = (int)e.ExpenseDate.DayOfWeek - (int)DayOfWeek.Monday;
                    if (diff < 0) diff += 7;
                    return e.ExpenseDate.Date.AddDays(-diff);
                });

            case AggregationPeriod.Monthly:
                return expenses.GroupBy(e => new DateTime(e.ExpenseDate.Year, e.ExpenseDate.Month, 1));

            case AggregationPeriod.Quarterly:
                return expenses.GroupBy(e =>
                {
                    var quarter = (e.ExpenseDate.Month - 1) / 3 + 1;
                    return new DateTime(e.ExpenseDate.Year, (quarter - 1) * 3 + 1, 1);
                });

            case AggregationPeriod.Yearly:
                return expenses.GroupBy(e => new DateTime(e.ExpenseDate.Year, 1, 1));

            default:
                return expenses.GroupBy(e => new DateTime(e.ExpenseDate.Year, e.ExpenseDate.Month, 1));
        }
    }

    private TrendStatisticsDto CalculateTrendStatistics(List<TrendPointDto> trendPoints)
    {
        if (!trendPoints.Any())
        {
            return new TrendStatisticsDto();
        }

        var totalAmount = trendPoints.Sum(tp => tp.Amount);
        var averagePerPeriod = totalAmount / trendPoints.Count;

        var sortedAmounts = trendPoints.Select(tp => tp.Amount).OrderBy(a => a).ToList();
        var medianPerPeriod = sortedAmounts.Count % 2 == 0
            ? (sortedAmounts[sortedAmounts.Count / 2 - 1] + sortedAmounts[sortedAmounts.Count / 2]) / 2
            : sortedAmounts[sortedAmounts.Count / 2];

        var minPoint = trendPoints.OrderBy(tp => tp.Amount).First();
        var maxPoint = trendPoints.OrderByDescending(tp => tp.Amount).First();

        var firstPoint = trendPoints.First();
        var lastPoint = trendPoints.Last();
        var growthRate = firstPoint.Amount > 0
            ? (lastPoint.Amount - firstPoint.Amount) / firstPoint.Amount * 100
            : 0;

        return new TrendStatisticsDto
        {
            TotalAmount = totalAmount,
            AveragePerPeriod = averagePerPeriod,
            MedianPerPeriod = medianPerPeriod,
            MinAmount = minPoint.Amount,
            MinDate = minPoint.Date,
            MaxAmount = maxPoint.Amount,
            MaxDate = maxPoint.Date,
            GrowthRate = growthRate
        };
    }

    private async Task<Result<byte[]>> ExportDataAsync<T>(T data, ExportFormat? format)
    {
        if (!format.HasValue)
        {
            return Result<byte[]>.Failure("Export format not specified");
        }

        try
        {
            byte[] exportData;
            IEnumerable<object> dataList = data is IEnumerable<object> enumerable
                ? enumerable
                : new object[] { data!};

            switch (format.Value)
            {
                case ExportFormat.Csv:
                    exportData = _exportService.ExportToCsv(dataList);
                    break;

                case ExportFormat.Pdf:
                    exportData = _exportService.ExportToPdf(dataList, "Expense Report",
                        $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    break;

                case ExportFormat.Excel:
                    exportData = _exportService.ExportToExcel(dataList, "Expense Report");
                    break;

                default:
                    return Result<byte[]>.Failure("Invalid export format");
            }

            return Result<byte[]>.Success(exportData);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Failed to export data: {ex.Message}");
        }
    }

    #endregion
}