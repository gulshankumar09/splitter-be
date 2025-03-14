using ExpenseService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IRecurringExpenseRepository
{
    Task<RecurringExpense?> GetByIdAsync(int id);
    Task<IEnumerable<RecurringExpense>> GetAllAsync();
    Task<IEnumerable<RecurringExpense>> GetByUserIdAsync(int userId);
    Task<IEnumerable<RecurringExpense>> GetByGroupIdAsync(int groupId);
    Task<IEnumerable<RecurringExpense>> GetActiveAsync();
    Task<IEnumerable<RecurringExpense>> GetDueForProcessingAsync(DateTime processingDate);
    Task<IEnumerable<RecurringExpense>> GetDueForRemindersAsync(DateTime reminderDate);
    Task AddAsync(RecurringExpense recurringExpense);
    void Update(RecurringExpense recurringExpense);
    void Delete(RecurringExpense recurringExpense);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}