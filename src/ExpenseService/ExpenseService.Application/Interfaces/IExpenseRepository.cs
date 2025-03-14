using ExpenseService.Domain.Entities;
using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(int id);
    Task<Expense?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Expense>> GetAllAsync();
    Task<IEnumerable<Expense>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Expense>> GetByCategoryAsync(ExpenseCategory category);
    Task<IEnumerable<Expense>> GetByTagAsync(string tag);
    Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Expense>> GetByGroupIdAsync(int groupId);
    Task AddAsync(Expense expense);
    void Update(Expense expense);
    void Delete(Expense expense);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}