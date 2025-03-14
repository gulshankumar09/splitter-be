using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.DTOs;

public class BudgetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? GroupId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }
    public List<BudgetCategoryDto> Categories { get; set; } = new();
    public BudgetNotificationThreshold? WarningThreshold { get; set; }
    public bool NotificationsEnabled { get; set; }
    public BudgetProgressDto Progress { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BudgetCategoryDto
{
    public int Id { get; set; }
    public ExpenseCategory Category { get; set; }
    public decimal? LimitAmount { get; set; }
    public BudgetProgressDto Progress { get; set; } = new();
}

public class BudgetProgressDto
{
    public decimal CurrentSpending { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal Percentage { get; set; }
    public bool IsOverBudget { get; set; }
    public List<ExpenseDto> RecentExpenses { get; set; } = new();
}

public class BudgetAlertDto
{
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal ThresholdPercentage { get; set; }
    public decimal CurrentSpending { get; set; }
    public decimal BudgetLimit { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBudgetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? GroupId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurring { get; set; }
    public List<BudgetCategoryRequest>? Categories { get; set; }
    public BudgetNotificationThreshold? WarningThreshold { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
}

public class UpdateBudgetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }
    public List<BudgetCategoryRequest>? Categories { get; set; }
    public BudgetNotificationThreshold? WarningThreshold { get; set; }
    public bool NotificationsEnabled { get; set; }
}

public class BudgetCategoryRequest
{
    public ExpenseCategory Category { get; set; }
    public decimal? LimitAmount { get; set; }
}

public class MarkBudgetAlertReadRequest
{
    public int AlertId { get; set; }
}