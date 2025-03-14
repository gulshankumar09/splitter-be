using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IExpenseService
{
    Task<Result<ExpenseDto>> GetExpenseByIdAsync(int id);
    Task<Result<IEnumerable<ExpenseDto>>> GetAllExpensesAsync();
    Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByUserIdAsync(int userId);
    Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByCategoryAsync(ExpenseCategory category);
    Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByTagAsync(string tag);
    Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Result<ExpenseDto>> CreateExpenseAsync(CreateExpenseRequest request);
    Task<Result<ExpenseDto>> UpdateExpenseAsync(int id, UpdateExpenseRequest request);
    Task<Result> DeleteExpenseAsync(int id);
    Task<Result<ExpenseDto>> AddAttachmentAsync(int expenseId, AddAttachmentRequest request);
    Task<Result<ExpenseDto>> RemoveAttachmentAsync(int expenseId, int attachmentId);
    Task<Result<ExpenseDto>> MarkSplitAsPaidAsync(int expenseId, MarkSplitPaidRequest request);
    Task<Result<ExpenseDto>> MarkSplitAsUnpaidAsync(int expenseId, MarkSplitPaidRequest request);
}