using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories;

public class RecurringExpenseRepository : GenericRepository<RecurringExpense>, IRecurringExpenseRepository
{
    private readonly ExpenseDbContext _context;

    public RecurringExpenseRepository(ExpenseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<RecurringExpense?> GetByIdAsync(int id)
    {
        return await _context.RecurringExpenses
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<RecurringExpense>> GetAllAsync()
    {
        return await _context.RecurringExpenses.ToListAsync();
    }

    public async Task<IEnumerable<RecurringExpense>> GetByUserIdAsync(int userId)
    {
        return await _context.RecurringExpenses
            .Where(r => r.CreatedByUserId == userId || r.ParticipantUserIds.Contains(userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<RecurringExpense>> GetByGroupIdAsync(int groupId)
    {
        return await _context.RecurringExpenses
            .Where(r => r.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RecurringExpense>> GetActiveAsync()
    {
        return await _context.RecurringExpenses
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<RecurringExpense>> GetDueForProcessingAsync(DateTime processingDate)
    {
        return await _context.RecurringExpenses
            .Where(r => r.IsActive && r.NextOccurrence <= processingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RecurringExpense>> GetDueForRemindersAsync(DateTime reminderDate)
    {
        return await _context.RecurringExpenses
            .Where(r => r.IsActive &&
                        r.SendReminders &&
                        r.NextOccurrence.AddDays(-r.ReminderDaysBefore) <= reminderDate &&
                        r.NextOccurrence > reminderDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.RecurringExpenses.AnyAsync(r => r.Id == id);
    }

    public void Update(RecurringExpense recurringExpense)
    {
        _context.RecurringExpenses.Update(recurringExpense);
    }

    public void Delete(RecurringExpense recurringExpense)
    {
        _context.RecurringExpenses.Remove(recurringExpense);
    }
}