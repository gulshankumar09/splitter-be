using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        IBudgetRepository budgetRepository,
        IExpenseRepository expenseRepository,
        ILogger<BudgetService> logger)
    {
        _budgetRepository = budgetRepository;
        _expenseRepository = expenseRepository;
        _logger = logger;
    }

    #region Budget CRUD Operations

    public async Task<Result<BudgetDto>> GetBudgetByIdAsync(int id)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {id} not found");
            }

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget with ID {Id}", id);
            return Result<BudgetDto>.Failure($"Error retrieving budget: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetDto>>> GetAllBudgetsAsync()
    {
        try
        {
            var budgets = await _budgetRepository.GetAllAsync();
            var budgetDtos = new List<BudgetDto>();

            foreach (var budget in budgets)
            {
                budgetDtos.Add(await MapToDtoWithProgressAsync(budget));
            }

            return Result<IEnumerable<BudgetDto>>.Success(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all budgets");
            return Result<IEnumerable<BudgetDto>>.Failure($"Error retrieving budgets: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetDto>>> GetBudgetsByUserIdAsync(int userId)
    {
        try
        {
            var budgets = await _budgetRepository.GetByUserIdAsync(userId);
            var budgetDtos = new List<BudgetDto>();

            foreach (var budget in budgets)
            {
                budgetDtos.Add(await MapToDtoWithProgressAsync(budget));
            }

            return Result<IEnumerable<BudgetDto>>.Success(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budgets for user ID {UserId}", userId);
            return Result<IEnumerable<BudgetDto>>.Failure($"Error retrieving user budgets: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetDto>>> GetBudgetsByGroupIdAsync(int groupId)
    {
        try
        {
            var budgets = await _budgetRepository.GetByGroupIdAsync(groupId);
            var budgetDtos = new List<BudgetDto>();

            foreach (var budget in budgets)
            {
                budgetDtos.Add(await MapToDtoWithProgressAsync(budget));
            }

            return Result<IEnumerable<BudgetDto>>.Success(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budgets for group ID {GroupId}", groupId);
            return Result<IEnumerable<BudgetDto>>.Failure($"Error retrieving group budgets: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetDto>>> GetActiveBudgetsAsync()
    {
        try
        {
            var budgets = await _budgetRepository.GetActiveAsync();
            var budgetDtos = new List<BudgetDto>();

            foreach (var budget in budgets)
            {
                budgetDtos.Add(await MapToDtoWithProgressAsync(budget));
            }

            return Result<IEnumerable<BudgetDto>>.Success(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active budgets");
            return Result<IEnumerable<BudgetDto>>.Failure($"Error retrieving active budgets: {ex.Message}");
        }
    }

    public async Task<Result<BudgetDto>> CreateBudgetAsync(CreateBudgetRequest request)
    {
        try
        {
            var budget = new Budget(
                request.Name,
                request.Description,
                request.UserId,
                request.Amount,
                request.CurrencyCode,
                request.Period,
                request.StartDate,
                request.EndDate,
                request.IsRecurring,
                request.GroupId);

            // Add categories if provided
            if (request.Categories != null && request.Categories.Any())
            {
                foreach (var categoryRequest in request.Categories)
                {
                    budget.AddCategory(categoryRequest.Category, categoryRequest.LimitAmount);
                }
            }

            // Set notification settings if provided
            if (request.WarningThreshold.HasValue)
            {
                budget.SetNotificationThreshold(request.WarningThreshold.Value);
            }

            budget.EnableNotifications(request.NotificationsEnabled);

            await _budgetRepository.AddAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget for user ID {UserId}", request.UserId);
            return Result<BudgetDto>.Failure($"Error creating budget: {ex.Message}");
        }
    }

    public async Task<Result<BudgetDto>> UpdateBudgetAsync(int id, UpdateBudgetRequest request)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {id} not found");
            }

            budget.Update(
                request.Name,
                request.Description,
                request.Amount,
                request.CurrencyCode,
                request.Period,
                request.StartDate,
                request.EndDate,
                request.IsRecurring,
                request.IsActive);

            // Update categories if provided
            if (request.Categories != null)
            {
                // Get current categories
                var currentCategories = budget.Categories.Select(c => c.Category).ToList();

                // Add new categories
                foreach (var categoryRequest in request.Categories)
                {
                    if (!currentCategories.Contains(categoryRequest.Category))
                    {
                        budget.AddCategory(categoryRequest.Category, categoryRequest.LimitAmount);
                    }
                    else
                    {
                        budget.UpdateCategoryLimit(categoryRequest.Category, categoryRequest.LimitAmount);
                    }
                }

                // Remove categories that no longer exist in the updated list
                var updatedCategories = request.Categories.Select(c => c.Category).ToList();
                foreach (var category in currentCategories)
                {
                    if (!updatedCategories.Contains(category))
                    {
                        budget.RemoveCategory(category);
                    }
                }
            }

            // Update notification settings
            if (request.WarningThreshold.HasValue)
            {
                budget.SetNotificationThreshold(request.WarningThreshold.Value);
            }

            budget.EnableNotifications(request.NotificationsEnabled);

            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget with ID {Id}", id);
            return Result<BudgetDto>.Failure($"Error updating budget: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteBudgetAsync(int id)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
            {
                return Result<bool>.Failure($"Budget with ID {id} not found");
            }

            _budgetRepository.Delete(budget);
            await _budgetRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget with ID {Id}", id);
            return Result<bool>.Failure($"Error deleting budget: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ActivateBudgetAsync(int id)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
            {
                return Result<bool>.Failure($"Budget with ID {id} not found");
            }

            budget.Activate();
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating budget with ID {Id}", id);
            return Result<bool>.Failure($"Error activating budget: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeactivateBudgetAsync(int id)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
            {
                return Result<bool>.Failure($"Budget with ID {id} not found");
            }

            budget.Deactivate();
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating budget with ID {Id}", id);
            return Result<bool>.Failure($"Error deactivating budget: {ex.Message}");
        }
    }

    #endregion

    #region Budget Category Operations

    public async Task<Result<BudgetCategoryDto>> GetBudgetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _budgetRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return Result<BudgetCategoryDto>.Failure($"Budget category with ID {id} not found");
            }

            // Get the budget for this category
            var budgetId = GetBudgetIdForCategory(category);
            if (!budgetId.HasValue)
            {
                return Result<BudgetCategoryDto>.Failure($"Cannot determine budget for category with ID {id}");
            }

            var categoryDto = await MapCategoryToDtoWithProgressAsync(category, budgetId.Value);
            return Result<BudgetCategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget category with ID {Id}", id);
            return Result<BudgetCategoryDto>.Failure($"Error retrieving budget category: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetCategoryDto>>> GetCategoriesByBudgetIdAsync(int budgetId)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<IEnumerable<BudgetCategoryDto>>.Failure($"Budget with ID {budgetId} not found");
            }

            var categoryDtos = new List<BudgetCategoryDto>();
            foreach (var category in budget.Categories)
            {
                categoryDtos.Add(await MapCategoryToDtoWithProgressAsync(category, budgetId));
            }

            return Result<IEnumerable<BudgetCategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories for budget ID {BudgetId}", budgetId);
            return Result<IEnumerable<BudgetCategoryDto>>.Failure($"Error retrieving budget categories: {ex.Message}");
        }
    }

    public async Task<Result<BudgetDto>> AddCategoryToBudgetAsync(int budgetId, BudgetCategoryRequest request)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {budgetId} not found");
            }

            budget.AddCategory(request.Category, request.LimitAmount);
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding category to budget ID {BudgetId}", budgetId);
            return Result<BudgetDto>.Failure($"Error adding category to budget: {ex.Message}");
        }
    }

    public async Task<Result<BudgetDto>> UpdateCategoryLimitAsync(int budgetId, ExpenseCategory category, decimal? limitAmount)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {budgetId} not found");
            }

            budget.UpdateCategoryLimit(category, limitAmount);
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category limit for budget ID {BudgetId}", budgetId);
            return Result<BudgetDto>.Failure($"Error updating category limit: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveCategoryFromBudgetAsync(int budgetId, ExpenseCategory category)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<bool>.Failure($"Budget with ID {budgetId} not found");
            }

            budget.RemoveCategory(category);
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing category from budget ID {BudgetId}", budgetId);
            return Result<bool>.Failure($"Error removing category from budget: {ex.Message}");
        }
    }

    #endregion

    #region Budget Progress and Analytics

    public async Task<Result<BudgetProgressDto>> GetBudgetProgressAsync(int budgetId)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetProgressDto>.Failure($"Budget with ID {budgetId} not found");
            }

            var progress = await CalculateBudgetProgressAsync(budget);
            return Result<BudgetProgressDto>.Success(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating progress for budget ID {BudgetId}", budgetId);
            return Result<BudgetProgressDto>.Failure($"Error calculating budget progress: {ex.Message}");
        }
    }

    public async Task<Result<BudgetCategoryDto>> GetCategoryProgressAsync(int budgetId, ExpenseCategory category)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetCategoryDto>.Failure($"Budget with ID {budgetId} not found");
            }

            var budgetCategory = budget.Categories.FirstOrDefault(c => c.Category == category);
            if (budgetCategory == null)
            {
                return Result<BudgetCategoryDto>.Failure($"Category {category} not found in budget with ID {budgetId}");
            }

            var categoryDto = await MapCategoryToDtoWithProgressAsync(budgetCategory, budgetId);
            return Result<BudgetCategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating progress for category {Category} in budget ID {BudgetId}", category, budgetId);
            return Result<BudgetCategoryDto>.Failure($"Error calculating category progress: {ex.Message}");
        }
    }

    public async Task<Result<BudgetDto>> RefreshBudgetProgressAsync(int budgetId)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {budgetId} not found");
            }

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing progress for budget ID {BudgetId}", budgetId);
            return Result<BudgetDto>.Failure($"Error refreshing budget progress: {ex.Message}");
        }
    }

    #endregion

    #region Budget Notifications & Alerts

    public async Task<Result<BudgetDto>> SetBudgetNotificationThresholdAsync(int budgetId, BudgetNotificationThreshold threshold)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {budgetId} not found");
            }

            budget.SetNotificationThreshold(threshold);
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification threshold for budget ID {BudgetId}", budgetId);
            return Result<BudgetDto>.Failure($"Error setting budget notification threshold: {ex.Message}");
        }
    }

    public async Task<Result<BudgetDto>> EnableBudgetNotificationsAsync(int budgetId, bool enabled)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<BudgetDto>.Failure($"Budget with ID {budgetId} not found");
            }

            budget.EnableNotifications(enabled);
            _budgetRepository.Update(budget);
            await _budgetRepository.SaveChangesAsync();

            var budgetDto = await MapToDtoWithProgressAsync(budget);
            return Result<BudgetDto>.Success(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error {Action} notifications for budget ID {BudgetId}",
                enabled ? "enabling" : "disabling", budgetId);
            return Result<BudgetDto>.Failure($"Error {(enabled ? "enabling" : "disabling")} budget notifications: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetAlertDto>>> GetAlertsByBudgetIdAsync(int budgetId)
    {
        try
        {
            var budget = await _budgetRepository.GetByIdAsync(budgetId);
            if (budget == null)
            {
                return Result<IEnumerable<BudgetAlertDto>>.Failure($"Budget with ID {budgetId} not found");
            }

            var alerts = await _budgetRepository.GetAlertsByBudgetIdAsync(budgetId);
            var alertDtos = alerts.Select(MapAlertToDto).ToList();

            return Result<IEnumerable<BudgetAlertDto>>.Success(alertDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts for budget ID {BudgetId}", budgetId);
            return Result<IEnumerable<BudgetAlertDto>>.Failure($"Error retrieving budget alerts: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BudgetAlertDto>>> GetUnreadAlertsByUserIdAsync(int userId)
    {
        try
        {
            var alerts = await _budgetRepository.GetUnreadAlertsByUserIdAsync(userId);
            var alertDtos = alerts.Select(MapAlertToDto).ToList();

            return Result<IEnumerable<BudgetAlertDto>>.Success(alertDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread alerts for user ID {UserId}", userId);
            return Result<IEnumerable<BudgetAlertDto>>.Failure($"Error retrieving unread alerts: {ex.Message}");
        }
    }

    public async Task<Result<bool>> MarkAlertAsReadAsync(int alertId)
    {
        try
        {
            var alert = await _budgetRepository.GetAlertByIdAsync(alertId);
            if (alert == null)
            {
                return Result<bool>.Failure($"Alert with ID {alertId} not found");
            }

            alert.MarkAsRead();
            _budgetRepository.UpdateAlert(alert);
            await _budgetRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking alert ID {AlertId} as read", alertId);
            return Result<bool>.Failure($"Error marking alert as read: {ex.Message}");
        }
    }

    #endregion

    #region Budget Processing

    public async Task<Result<bool>> ProcessBudgetsAsync()
    {
        try
        {
            // First, check all active budgets for threshold alerts
            await CheckBudgetThresholdsAsync();

            // Then, renew any recurring budgets that have ended
            await RenewRecurringBudgetsAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing budgets");
            return Result<bool>.Failure($"Error processing budgets: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CheckBudgetThresholdsAsync(int? budgetId = null)
    {
        try
        {
            IEnumerable<Budget> budgetsToCheck;

            if (budgetId.HasValue)
            {
                // Check a specific budget
                var budget = await _budgetRepository.GetByIdAsync(budgetId.Value);
                if (budget == null)
                {
                    return Result<bool>.Failure($"Budget with ID {budgetId} not found");
                }

                budgetsToCheck = new List<Budget> { budget };
            }
            else
            {
                // Check all active budgets
                budgetsToCheck = await _budgetRepository.GetActiveAsync();
            }

            foreach (var budget in budgetsToCheck)
            {
                if (!budget.NotificationsEnabled || !budget.WarningThreshold.HasValue)
                {
                    continue;
                }

                var progress = await CalculateBudgetProgressAsync(budget);

                // If spending percentage exceeds threshold and notifications are enabled, create alert
                if (progress.Percentage >= (int)budget.WarningThreshold.Value)
                {
                    // Check if we already have an alert for this threshold
                    var existingAlerts = await _budgetRepository.GetAlertsByBudgetIdAsync(budget.Id);

                    if (!existingAlerts.Any(a =>
                        a.ThresholdPercentage == (int)budget.WarningThreshold.Value &&
                        !a.IsRead))
                    {
                        // Create a new alert
                        var message = $"Your budget \"{budget.Name}\" has reached {(int)budget.WarningThreshold.Value}% of its limit";

                        var alert = new BudgetAlert(
                            budget.Id,
                            budget.UserId,
                            message,
                            (int)budget.WarningThreshold.Value,
                            progress.CurrentSpending,
                            budget.Amount);

                        await _budgetRepository.AddAlertAsync(alert);
                        await _budgetRepository.SaveChangesAsync();
                    }
                }
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking budget thresholds");
            return Result<bool>.Failure($"Error checking budget thresholds: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RenewRecurringBudgetsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var activeBudgets = await _budgetRepository.GetActiveAsync();

            // Check for recurring budgets that have ended
            var budgetsToRenew = activeBudgets.Where(b =>
                b.IsRecurring &&
                b.EndDate < now).ToList();

            foreach (var budget in budgetsToRenew)
            {
                // Calculate new period dates
                var newStartDate = budget.GetNextPeriodStartDate();
                var newEndDate = budget.Period switch
                {
                    BudgetPeriod.Daily => newStartDate.AddDays(1).AddSeconds(-1),
                    BudgetPeriod.Weekly => newStartDate.AddDays(7).AddSeconds(-1),
                    BudgetPeriod.Monthly => newStartDate.AddMonths(1).AddSeconds(-1),
                    BudgetPeriod.Quarterly => newStartDate.AddMonths(3).AddSeconds(-1),
                    BudgetPeriod.Yearly => newStartDate.AddYears(1).AddSeconds(-1),
                    _ => newStartDate.AddMonths(1).AddSeconds(-1)
                };

                // Create a new budget with the same settings
                var newBudget = new Budget(
                    budget.Name,
                    budget.Description,
                    budget.UserId,
                    budget.Amount,
                    budget.CurrencyCode,
                    budget.Period,
                    newStartDate,
                    newEndDate,
                    budget.IsRecurring,
                    budget.GroupId);

                // Copy notification settings
                if (budget.WarningThreshold.HasValue)
                {
                    newBudget.SetNotificationThreshold(budget.WarningThreshold.Value);
                }

                newBudget.EnableNotifications(budget.NotificationsEnabled);

                // Copy categories
                foreach (var category in budget.Categories)
                {
                    newBudget.AddCategory(category.Category, category.LimitAmount);
                }

                // Save the new budget
                await _budgetRepository.AddAsync(newBudget);

                // Deactivate the old budget
                budget.Deactivate();
                _budgetRepository.Update(budget);
            }

            if (budgetsToRenew.Any())
            {
                await _budgetRepository.SaveChangesAsync();
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing recurring budgets");
            return Result<bool>.Failure($"Error renewing recurring budgets: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<BudgetDto> MapToDtoWithProgressAsync(Budget budget)
    {
        var progress = await CalculateBudgetProgressAsync(budget);

        var dto = new BudgetDto
        {
            Id = budget.Id,
            Name = budget.Name,
            Description = budget.Description,
            UserId = budget.UserId,
            GroupId = budget.GroupId,
            Amount = budget.Amount,
            CurrencyCode = budget.CurrencyCode,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            IsRecurring = budget.IsRecurring,
            IsActive = budget.IsActive,
            WarningThreshold = budget.WarningThreshold,
            NotificationsEnabled = budget.NotificationsEnabled,
            Progress = progress,
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt
        };

        // Add categories with progress
        foreach (var category in budget.Categories)
        {
            dto.Categories.Add(await MapCategoryToDtoWithProgressAsync(category, budget.Id));
        }

        return dto;
    }

    private async Task<BudgetCategoryDto> MapCategoryToDtoWithProgressAsync(BudgetCategory category, int budgetId)
    {
        var progress = await CalculateCategoryProgressAsync(budgetId, category);

        return new BudgetCategoryDto
        {
            Id = category.Id,
            Category = category.Category,
            LimitAmount = category.LimitAmount,
            Progress = progress
        };
    }

    private BudgetAlertDto MapAlertToDto(BudgetAlert alert)
    {
        return new BudgetAlertDto
        {
            Id = alert.Id,
            BudgetId = alert.BudgetId,
            BudgetName = "", // Would need to retrieve from budget repository
            UserId = alert.UserId,
            Message = alert.Message,
            ThresholdPercentage = alert.ThresholdPercentage,
            CurrentSpending = alert.CurrentSpending,
            BudgetLimit = alert.BudgetLimit,
            IsRead = alert.IsRead,
            ReadAt = alert.ReadAt,
            CreatedAt = alert.CreatedAt
        };
    }

    private async Task<BudgetProgressDto> CalculateBudgetProgressAsync(Budget budget)
    {
        // Get all expenses within the budget period that match the budget criteria
        var expenses = await GetExpensesForBudgetAsync(budget);

        // Calculate spending
        var currentSpending = expenses.Sum(e => e.Amount);
        var remainingAmount = budget.Amount - currentSpending;
        var percentage = budget.Amount > 0
            ? Math.Min(100, Math.Round((currentSpending / budget.Amount) * 100, 2))
            : 0;

        return new BudgetProgressDto
        {
            CurrentSpending = currentSpending,
            RemainingAmount = remainingAmount,
            Percentage = percentage,
            IsOverBudget = currentSpending > budget.Amount,
            RecentExpenses = expenses.OrderByDescending(e => e.ExpenseDate)
                .Take(5)
                .Select(MapExpenseToDto)
                .ToList()
        };
    }

    private async Task<BudgetProgressDto> CalculateCategoryProgressAsync(int budgetId, BudgetCategory category)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId);
        if (budget == null)
        {
            return new BudgetProgressDto();
        }

        // Get expenses for this category in the budget period
        var expenses = await GetExpensesForBudgetAsync(budget, category.Category);

        var currentSpending = expenses.Sum(e => e.Amount);
        var limitAmount = category.LimitAmount ?? budget.Amount;
        var remainingAmount = limitAmount - currentSpending;
        var percentage = limitAmount > 0
            ? Math.Min(100, Math.Round((currentSpending / limitAmount) * 100, 2))
            : 0;

        return new BudgetProgressDto
        {
            CurrentSpending = currentSpending,
            RemainingAmount = remainingAmount,
            Percentage = percentage,
            IsOverBudget = currentSpending > limitAmount,
            RecentExpenses = expenses.OrderByDescending(e => e.ExpenseDate)
                .Take(5)
                .Select(MapExpenseToDto)
                .ToList()
        };
    }

    private async Task<IEnumerable<Expense>> GetExpensesForBudgetAsync(Budget budget, ExpenseCategory? category = null)
    {
        // Get expenses within the budget date range
        var expenses = await _expenseRepository.GetByDateRangeAsync(budget.StartDate, budget.EndDate);

        // Filter by user or group
        if (budget.GroupId.HasValue)
        {
            expenses = expenses.Where(e => e.GroupId == budget.GroupId.Value);
        }
        else
        {
            // For personal budgets, only include expenses paid by the user that aren't part of a group
            expenses = expenses.Where(e => e.PaidByUserId == budget.UserId && !e.GroupId.HasValue);
        }

        // Filter by specific category if provided
        if (category.HasValue)
        {
            expenses = expenses.Where(e => e.Category == category.Value);
        }
        else if (budget.Categories.Any())
        {
            // If budget has specific categories, only include those
            var budgetCategories = budget.Categories.Select(c => c.Category).ToList();
            expenses = expenses.Where(e => budgetCategories.Contains(e.Category));
        }

        // Convert to same currency if needed
        // This would require currency conversion service, which is out of scope for this implementation

        return expenses;
    }

    private ExpenseDto MapExpenseToDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            CurrencyCode = expense.CurrencyCode,
            OriginalAmount = expense.OriginalAmount,
            OriginalCurrencyCode = expense.OriginalCurrencyCode,
            PaidByUserId = expense.PaidByUserId,
            Category = expense.Category,
            Notes = expense.Notes,
            ExpenseDate = expense.ExpenseDate,
            GroupId = expense.GroupId,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }

    private int? GetBudgetIdForCategory(BudgetCategory category)
    {
        // Reflection to get the shadow property value
        // In real implementation, you might want to join with the budget table
        // or have a navigation property on BudgetCategory

        // This is a simplified approach for the sake of this implementation
        return null;
    }

    #endregion
}