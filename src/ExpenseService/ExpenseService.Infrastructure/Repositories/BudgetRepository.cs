using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ExpenseDbContext _context;

    public BudgetRepository(ExpenseDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Budget>> GetAllAsync()
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetByUserIdAsync(int userId)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetByGroupIdAsync(int groupId)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetActiveAsync()
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetActiveByUserIdAsync(int userId)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.UserId == userId && b.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetActiveByGroupIdAsync(int groupId)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.GroupId == groupId && b.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetByPeriodAsync(BudgetPeriod period)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.Period == period && b.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Budget>> GetForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b =>
                b.IsActive &&
                ((b.StartDate <= endDate && b.EndDate >= startDate) ||
                 (b.IsRecurring && b.StartDate <= endDate)))
            .ToListAsync();
    }

    public async Task AddAsync(Budget budget)
    {
        await _context.Budgets.AddAsync(budget);
    }

    public void Update(Budget budget)
    {
        _context.Budgets.Update(budget);
    }

    public void Delete(Budget budget)
    {
        _context.Budgets.Remove(budget);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Budgets.AnyAsync(b => b.Id == id);
    }

    public async Task<BudgetCategory?> GetCategoryByIdAsync(int id)
    {
        return await _context.BudgetCategories.FindAsync(id);
    }

    public async Task<IEnumerable<BudgetCategory>> GetCategoriesByBudgetIdAsync(int budgetId)
    {
        return await _context.BudgetCategories
            .Where(bc => EF.Property<int>(bc, "BudgetId") == budgetId)
            .ToListAsync();
    }

    public async Task<BudgetAlert?> GetAlertByIdAsync(int id)
    {
        return await _context.BudgetAlerts.FindAsync(id);
    }

    public async Task<IEnumerable<BudgetAlert>> GetAlertsByBudgetIdAsync(int budgetId)
    {
        return await _context.BudgetAlerts
            .Where(ba => ba.BudgetId == budgetId)
            .OrderByDescending(ba => ba.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetAlert>> GetUnreadAlertsByUserIdAsync(int userId)
    {
        return await _context.BudgetAlerts
            .Where(ba => ba.UserId == userId && !ba.IsRead)
            .OrderByDescending(ba => ba.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAlertAsync(BudgetAlert alert)
    {
        await _context.BudgetAlerts.AddAsync(alert);
    }

    public void UpdateAlert(BudgetAlert alert)
    {
        _context.BudgetAlerts.Update(alert);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}