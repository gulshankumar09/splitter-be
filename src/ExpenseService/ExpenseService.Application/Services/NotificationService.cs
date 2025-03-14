using ExpenseService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationOptions _options;
    private readonly IUserRepository _userRepository;

    public NotificationService(
        ILogger<NotificationService> logger,
        IOptions<NotificationOptions> options,
        IUserRepository userRepository)
    {
        _logger = logger;
        _options = options.Value;
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> SendBudgetAlertAsync(int userId, string message)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure($"User with ID {userId} not found");
            }

            if (string.IsNullOrEmpty(user.DeviceToken))
            {
                _logger.LogWarning("User {UserId} has no device token for notifications", userId);
                return Result<bool>.Failure("User has no registered device for notifications");
            }

            // TODO: Implement actual push notification sending logic
            // This could integrate with Firebase Cloud Messaging, Azure Notification Hubs, etc.

            _logger.LogInformation("Budget alert sent to user {UserId}: {Message}", userId, message);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending budget alert to user {UserId}", userId);
            return Result<bool>.Failure($"Error sending notification: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SendExpenseReminderAsync(int userId, string message)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure($"User with ID {userId} not found");
            }

            if (string.IsNullOrEmpty(user.DeviceToken))
            {
                _logger.LogWarning("User {UserId} has no device token for notifications", userId);
                return Result<bool>.Failure("User has no registered device for notifications");
            }

            // TODO: Implement actual push notification sending logic

            _logger.LogInformation("Expense reminder sent to user {UserId}: {Message}", userId, message);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending expense reminder to user {UserId}", userId);
            return Result<bool>.Failure($"Error sending notification: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUserDeviceTokenAsync(int userId, string deviceToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure($"User with ID {userId} not found");
            }

            user.UpdateDeviceToken(deviceToken);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device token for user {UserId}", userId);
            return Result<bool>.Failure($"Error updating device token: {ex.Message}");
        }
    }
}

public class NotificationOptions
{
    public string FirebaseServerKey { get; set; }
    public string AzureNotificationHubConnectionString { get; set; }
    public string AzureNotificationHubName { get; set; }
    public bool EnablePushNotifications { get; set; } = true;
}