using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IBudgetService
{
    // Budget CRUD Operations
    Task<Result<BudgetDto>> GetBudgetByIdAsync(int id);
    Task<Result<IEnumerable<BudgetDto>>> GetAllBudgetsAsync();
    Task<Result<IEnumerable<BudgetDto>>> GetBudgetsByUserIdAsync(int userId);
    Task<Result<IEnumerable<BudgetDto>>> GetBudgetsByGroupIdAsync(int groupId);
    Task<Result<IEnumerable<BudgetDto>>> GetActiveBudgetsAsync();
    Task<Result<BudgetDto>> CreateBudgetAsync(CreateBudgetRequest request);
    Task<Result<BudgetDto>> UpdateBudgetAsync(int id, UpdateBudgetRequest request);
    Task<Result<bool>> DeleteBudgetAsync(int id);
    Task<Result<bool>> ActivateBudgetAsync(int id);
    Task<Result<bool>> DeactivateBudgetAsync(int id);

    // Budget Category Operations
    Task<Result<BudgetCategoryDto>> GetBudgetCategoryByIdAsync(int id);
    Task<Result<IEnumerable<BudgetCategoryDto>>> GetCategoriesByBudgetIdAsync(int budgetId);
    Task<Result<BudgetDto>> AddCategoryToBudgetAsync(int budgetId, BudgetCategoryRequest request);
    Task<Result<BudgetDto>> UpdateCategoryLimitAsync(int budgetId, ExpenseCategory category, decimal? limitAmount);
    Task<Result<bool>> RemoveCategoryFromBudgetAsync(int budgetId, ExpenseCategory category);

    // Budget Progress and Analytics
    Task<Result<BudgetProgressDto>> GetBudgetProgressAsync(int budgetId);
    Task<Result<BudgetCategoryDto>> GetCategoryProgressAsync(int budgetId, ExpenseCategory category);
    Task<Result<BudgetDto>> RefreshBudgetProgressAsync(int budgetId);

    // Budget Notifications & Alerts
    Task<Result<BudgetDto>> SetBudgetNotificationThresholdAsync(int budgetId, BudgetNotificationThreshold threshold);
    Task<Result<BudgetDto>> EnableBudgetNotificationsAsync(int budgetId, bool enabled);
    Task<Result<IEnumerable<BudgetAlertDto>>> GetAlertsByBudgetIdAsync(int budgetId);
    Task<Result<IEnumerable<BudgetAlertDto>>> GetUnreadAlertsByUserIdAsync(int userId);
    Task<Result<bool>> MarkAlertAsReadAsync(int alertId);

    // Budget Processing
    Task<Result<bool>> ProcessBudgetsAsync();
    Task<Result<bool>> CheckBudgetThresholdsAsync(int? budgetId = null);
    Task<Result<bool>> RenewRecurringBudgetsAsync();
}