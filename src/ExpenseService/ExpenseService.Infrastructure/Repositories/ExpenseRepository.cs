using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories;

namespace ExpenseService.Infrastructure.Repositories;

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    private readonly ExpenseDbContext _context;

    public ExpenseRepository(ExpenseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expense?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public override async Task<IEnumerable<Expense>> GetAllAsync()
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByUserIdAsync(int userId)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .Where(e => e.PaidByUserId == userId || e.Splits.Any(s => s.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByCategoryAsync(ExpenseCategory category)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .Where(e => e.Category == category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByTagAsync(string tag)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .Where(e => e.Tags.Contains(tag))
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByGroupIdAsync(int groupId)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
            .Include(e => e.Attachments)
            .Where(e => e.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Expenses.AnyAsync(e => e.Id == id);
    }

    public void Update(Expense expense)
    {
        _context.Expenses.Update(expense);
    }

    public void Delete(Expense expense)
    {
        _context.Expenses.Remove(expense);
    }
}