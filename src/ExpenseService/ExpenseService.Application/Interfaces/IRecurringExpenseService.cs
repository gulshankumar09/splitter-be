using ExpenseService.Application.DTOs;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IRecurringExpenseService
{
    Task<Result<RecurringExpenseDto>> GetRecurringExpenseByIdAsync(int id);
    Task<Result<IEnumerable<RecurringExpenseDto>>> GetAllRecurringExpensesAsync();
    Task<Result<IEnumerable<RecurringExpenseDto>>> GetRecurringExpensesByUserIdAsync(int userId);
    Task<Result<IEnumerable<RecurringExpenseDto>>> GetRecurringExpensesByGroupIdAsync(int groupId);
    Task<Result<RecurringExpenseDto>> CreateRecurringExpenseAsync(CreateRecurringExpenseRequest request);
    Task<Result<RecurringExpenseDto>> UpdateRecurringExpenseAsync(int id, UpdateRecurringExpenseRequest request);
    Task<Result<RecurringExpenseDto>> UpdateNotificationSettingsAsync(int id, UpdateNotificationSettingsRequest request);
    Task<Result<RecurringExpenseDto>> ActivateRecurringExpenseAsync(int id);
    Task<Result<RecurringExpenseDto>> DeactivateRecurringExpenseAsync(int id);
    Task<Result> DeleteRecurringExpenseAsync(int id);
    Task<Result> ProcessDueRecurringExpensesAsync();
    Task<Result<List<RecurringExpenseReminderDto>>> GetUpcomingExpenseRemindersAsync(int userId);
}