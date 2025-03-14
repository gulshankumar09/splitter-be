using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.DTOs;

public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ExpenseSummaryDto
{
    public DateRange Period { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalExpenses { get; set; }
    public decimal AverageExpenseAmount { get; set; }
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
    public List<MonthlyBreakdownDto> MonthlyBreakdown { get; set; } = new();
}

public class CategoryBreakdownDto
{
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int ExpenseCount { get; set; }
}

public class MonthlyBreakdownDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ExpenseCount { get; set; }
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
}

public class SpendingTrendDto
{
    public List<TrendPointDto> TrendPoints { get; set; } = new();
    public TrendStatisticsDto Statistics { get; set; } = new();
}

public class TrendPointDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int ExpenseCount { get; set; }
}

public class TrendStatisticsDto
{
    public decimal TotalAmount { get; set; }
    public decimal AveragePerPeriod { get; set; }
    public decimal MedianPerPeriod { get; set; }
    public decimal MinAmount { get; set; }
    public DateTime MinDate { get; set; }
    public decimal MaxAmount { get; set; }
    public DateTime MaxDate { get; set; }
    public decimal GrowthRate { get; set; } // Percentage change from start to end
}

public class ExpenseReportRequestDto
{
    public int? UserId { get; set; }
    public int? GroupId { get; set; }
    public DateRange Period { get; set; } = new();
    public List<ExpenseCategory>? Categories { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public ReportType ReportType { get; set; } = ReportType.Summary;
    public AggregationPeriod AggregationPeriod { get; set; } = AggregationPeriod.Monthly;
    public ExportFormat? ExportFormat { get; set; }
}

public enum ReportType
{
    Summary,
    CategoryBreakdown,
    SpendingTrend
}

public enum AggregationPeriod
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public enum ExportFormat
{
    Csv,
    Pdf,
    Excel
}