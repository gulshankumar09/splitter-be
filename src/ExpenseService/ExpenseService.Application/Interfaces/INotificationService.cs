using SharedLibrary.Models;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface INotificationService
{
    Task<Result<bool>> SendBudgetAlertAsync(int userId, string message);
    Task<Result<bool>> SendExpenseReminderAsync(int userId, string message);
    Task<Result<bool>> UpdateUserDeviceTokenAsync(int userId, string deviceToken);
}