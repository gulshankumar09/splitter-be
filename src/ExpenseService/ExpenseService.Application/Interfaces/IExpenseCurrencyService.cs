using ExpenseService.Application.DTOs;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IExpenseCurrencyService
{
    /// <summary>
    /// Converts an expense amount to the specified currency
    /// </summary>
    Task<ExpenseDto> ConvertExpenseCurrencyAsync(int expenseId, string toCurrencyCode);

    /// <summary>
    /// Converts an expense to the user's default currency based on their preferences
    /// </summary>
    Task<ExpenseDto> ConvertExpenseToUserCurrencyAsync(int expenseId, int userId);

    /// <summary>
    /// Converts an expense to the group's default currency based on group preferences
    /// </summary>
    Task<ExpenseDto> ConvertExpenseToGroupCurrencyAsync(int expenseId, int groupId);

    /// <summary>
    /// Convert all expenses in a group to the group's default currency
    /// </summary>
    Task ConvertAllGroupExpensesToGroupCurrencyAsync(int groupId);

    /// <summary>
    /// Gets the appropriate currency for a new expense based on user and group preferences
    /// </summary>
    Task<string> GetDefaultExpenseCurrencyAsync(int userId, int? groupId = null);
}