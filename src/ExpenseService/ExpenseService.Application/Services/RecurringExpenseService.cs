using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class RecurringExpenseService : IRecurringExpenseService
{
    private readonly IRecurringExpenseRepository _recurringExpenseRepository;
    private readonly IExpenseRepository _expenseRepository;

    public RecurringExpenseService(
        IRecurringExpenseRepository recurringExpenseRepository,
        IExpenseRepository expenseRepository)
    {
        _recurringExpenseRepository = recurringExpenseRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<RecurringExpenseDto>> GetRecurringExpenseByIdAsync(int id)
    {
        var recurringExpense = await _recurringExpenseRepository.GetByIdAsync(id);
        if (recurringExpense == null)
        {
            return Result<RecurringExpenseDto>.Failure("Recurring expense not found");
        }

        return Result<RecurringExpenseDto>.Success(MapToDto(recurringExpense));
    }

    public async Task<Result<IEnumerable<RecurringExpenseDto>>> GetAllRecurringExpensesAsync()
    {
        var recurringExpenses = await _recurringExpenseRepository.GetAllAsync();
        var recurringExpenseDtos = recurringExpenses.Select(MapToDto);
        return Result<IEnumerable<RecurringExpenseDto>>.Success(recurringExpenseDtos);
    }

    public async Task<Result<IEnumerable<RecurringExpenseDto>>> GetRecurringExpensesByUserIdAsync(int userId)
    {
        var recurringExpenses = await _recurringExpenseRepository.GetByUserIdAsync(userId);
        var recurringExpenseDtos = recurringExpenses.Select(MapToDto);
        return Result<IEnumerable<RecurringExpenseDto>>.Success(recurringExpenseDtos);
    }

    public async Task<Result<IEnumerable<RecurringExpenseDto>>> GetRecurringExpensesByGroupIdAsync(int groupId)
    {
        var recurringExpenses = await _recurringExpenseRepository.GetByGroupIdAsync(groupId);
        var recurringExpenseDtos = recurringExpenses.Select(MapToDto);
        return Result<IEnumerable<RecurringExpenseDto>>.Success(recurringExpenseDtos);
    }

    public async Task<Result<RecurringExpenseDto>> CreateRecurringExpenseAsync(CreateRecurringExpenseRequest request)
    {
        try
        {
            var recurringExpense = new RecurringExpense(
                request.Title,
                request.Description,
                request.Amount,
                request.CreatedByUserId,
                request.Category,
                request.Frequency,
                request.StartDate,
                request.ParticipantUserIds,
                request.SplitType,
                request.FrequencyParam,
                request.EndDate,
                request.GroupId);

            recurringExpense.UpdateNotificationSettings(request.SendReminders, request.ReminderDaysBefore);

            await _recurringExpenseRepository.AddAsync(recurringExpense);
            await _recurringExpenseRepository.SaveChangesAsync();

            return Result<RecurringExpenseDto>.Success(MapToDto(recurringExpense));
        }
        catch (Exception ex)
        {
            return Result<RecurringExpenseDto>.Failure($"Failed to create recurring expense: {ex.Message}");
        }
    }

    public async Task<Result<RecurringExpenseDto>> UpdateRecurringExpenseAsync(int id, UpdateRecurringExpenseRequest request)
    {
        try
        {
            var recurringExpense = await _recurringExpenseRepository.GetByIdAsync(id);
            if (recurringExpense == null)
            {
                return Result<RecurringExpenseDto>.Failure("Recurring expense not found");
            }

            recurringExpense.UpdateDetails(
                request.Title,
                request.Description,
                request.Amount,
                request.Category,
                request.Frequency,
                request.StartDate,
                request.ParticipantUserIds,
                request.SplitType,
                request.FrequencyParam,
                request.EndDate,
                request.GroupId);

            _recurringExpenseRepository.Update(recurringExpense);
            await _recurringExpenseRepository.SaveChangesAsync();

            return Result<RecurringExpenseDto>.Success(MapToDto(recurringExpense));
        }
        catch (Exception ex)
        {
            return Result<RecurringExpenseDto>.Failure($"Failed to update recurring expense: {ex.Message}");
        }
    }

    public async Task<Result<RecurringExpenseDto>> UpdateNotificationSettingsAsync(int id, UpdateNotificationSettingsRequest request)
    {
        try
        {
            var recurringExpense = await _recurringExpenseRepository.GetByIdAsync(id);
            if (recurringExpense == null)
            {
                return Result<RecurringExpenseDto>.Failure("Recurring expense not found");
            }

            recurringExpense.UpdateNotificationSettings(request.SendReminders, request.ReminderDaysBefore);

            _recurringExpenseRepository.Update(recurringExpense);
            await _recurringExpenseRepository.SaveChangesAsync();

            return Result<RecurringExpenseDto>.Success(MapToDto(recurringExpense));
        }
        catch (Exception ex)
        {
            return Result<RecurringExpenseDto>.Failure($"Failed to update notification settings: {ex.Message}");
        }
    }

    public async Task<Result<RecurringExpenseDto>> ActivateRecurringExpenseAsync(int id)
    {
        try
        {
            var recurringExpense = await _recurringExpenseRepository.GetByIdAsync(id);
            if (recurringExpense == null)
            {
                return Result<RecurringExpenseDto>.Failure("Recurring expense not found");
            }

            recurringExpense.Activate();

            _recurringExpenseRepository.Update(recurringExpense);
            await _recurringExpenseRepository.SaveChangesAsync();

            return Result<RecurringExpenseDto>.Success(MapToDto(recurringExpense));
        }
        catch (Exception ex)
        {
            return Result<RecurringExpenseDto>.Failure($"Failed to activate recurring expense: {ex.Message}");
        }
    }

    public async Task<Result<RecurringExpenseDto>> DeactivateRecurringExpenseAsync(int id)
    {
        try
        {
            var recurringExpense = await _recurringExpenseRepository.GetByIdAsync(id);
            if (recurringExpense == null)
            {
                return Result<RecurringExpenseDto>.Failure("Recurring expense not found");
            }

            recurringExpense.Deactivate();

            _recurringExpenseRepository.Update(recurringExpense);
            await _recurringExpenseRepository.SaveChangesAsync();

            return Result<RecurringExpenseDto>.Success(MapToDto(recurringExpense));
        }
        catch (Exception ex)
        {
            return Result<RecurringExpenseDto>.Failure($"Failed to deactivate recurring expense: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRecurringExpenseAsync(int id)
    {
        try
        {
            var recurringExpense = await _recurringExpenseRepository.GetByIdAsync(id);
            if (recurringExpense == null)
            {
                return Result.Failure("Recurring expense not found");
            }

            _recurringExpenseRepository.Delete(recurringExpense);
            await _recurringExpenseRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete recurring expense: {ex.Message}");
        }
    }

    public async Task<Result> ProcessDueRecurringExpensesAsync()
    {
        try
        {
            var dueRecurringExpenses = await _recurringExpenseRepository.GetDueForProcessingAsync(DateTime.Today);
            if (!dueRecurringExpenses.Any())
            {
                return Result.Success("No recurring expenses due for processing");
            }

            int processedCount = 0;
            foreach (var recurringExpense in dueRecurringExpenses)
            {
                // Generate and save the expense
                var expense = recurringExpense.GenerateExpense();
                await _expenseRepository.AddAsync(expense);

                // Update the recurring expense
                _recurringExpenseRepository.Update(recurringExpense);

                processedCount++;
            }

            await _recurringExpenseRepository.SaveChangesAsync();
            await _expenseRepository.SaveChangesAsync();

            return Result.Success($"Successfully processed {processedCount} recurring expenses");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to process recurring expenses: {ex.Message}");
        }
    }

    public async Task<Result<List<RecurringExpenseReminderDto>>> GetUpcomingExpenseRemindersAsync(int userId)
    {
        try
        {
            var recurringExpenses = await _recurringExpenseRepository.GetByUserIdAsync(userId);
            var upcomingReminders = recurringExpenses
                .Where(r => r.IsActive && r.ShouldSendReminder())
                .Select(r => new RecurringExpenseReminderDto
                {
                    RecurringExpenseId = r.Id,
                    Title = r.Title,
                    Amount = r.Amount,
                    NextOccurrenceDate = r.NextOccurrence,
                    DaysRemaining = (r.NextOccurrence - DateTime.Today).Days
                })
                .OrderBy(r => r.DaysRemaining)
                .ToList();

            return Result<List<RecurringExpenseReminderDto>>.Success(upcomingReminders);
        }
        catch (Exception ex)
        {
            return Result<List<RecurringExpenseReminderDto>>.Failure($"Failed to get upcoming reminders: {ex.Message}");
        }
    }

    private RecurringExpenseDto MapToDto(RecurringExpense recurringExpense)
    {
        return new RecurringExpenseDto
        {
            Id = recurringExpense.Id,
            Title = recurringExpense.Title,
            Description = recurringExpense.Description,
            Amount = recurringExpense.Amount,
            CreatedByUserId = recurringExpense.CreatedByUserId,
            Category = recurringExpense.Category,
            Frequency = recurringExpense.Frequency,
            FrequencyParam = recurringExpense.FrequencyParam,
            StartDate = recurringExpense.StartDate,
            EndDate = recurringExpense.EndDate,
            NextOccurrence = recurringExpense.NextOccurrence,
            LastProcessed = recurringExpense.LastProcessed,
            IsActive = recurringExpense.IsActive,
            ParticipantUserIds = recurringExpense.ParticipantUserIds,
            GroupId = recurringExpense.GroupId,
            SplitType = recurringExpense.SplitType,
            SendReminders = recurringExpense.SendReminders,
            ReminderDaysBefore = recurringExpense.ReminderDaysBefore,
            CreatedAt = recurringExpense.CreatedAt
        };
    }
}