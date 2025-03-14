using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(int id);
    Task<IEnumerable<Budget>> GetAllAsync();
    Task<IEnumerable<Budget>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Budget>> GetByGroupIdAsync(int groupId);
    Task<IEnumerable<Budget>> GetActiveAsync();
    Task<IEnumerable<Budget>> GetActiveByUserIdAsync(int userId);
    Task<IEnumerable<Budget>> GetActiveByGroupIdAsync(int groupId);
    Task<IEnumerable<Budget>> GetByPeriodAsync(BudgetPeriod period);
    Task<IEnumerable<Budget>> GetForDateRangeAsync(DateTime startDate, DateTime endDate);

    Task AddAsync(Budget budget);
    void Update(Budget budget);
    void Delete(Budget budget);
    Task<bool> ExistsAsync(int id);

    // Budget Categories
    Task<BudgetCategory?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<BudgetCategory>> GetCategoriesByBudgetIdAsync(int budgetId);

    // Budget Alerts
    Task<BudgetAlert?> GetAlertByIdAsync(int id);
    Task<IEnumerable<BudgetAlert>> GetAlertsByBudgetIdAsync(int budgetId);
    Task<IEnumerable<BudgetAlert>> GetUnreadAlertsByUserIdAsync(int userId);
    Task AddAlertAsync(BudgetAlert alert);
    void UpdateAlert(BudgetAlert alert);

    Task SaveChangesAsync();
}