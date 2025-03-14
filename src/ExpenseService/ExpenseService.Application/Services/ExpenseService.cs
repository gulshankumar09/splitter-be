using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using SharedLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<ExpenseDto>> GetExpenseByIdAsync(int id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            return Result<ExpenseDto>.Failure("Expense not found");
        }

        return Result<ExpenseDto>.Success(MapToDto(expense));
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetAllExpensesAsync()
    {
        var expenses = await _expenseRepository.GetAllAsync();
        var expenseDtos = expenses.Select(MapToDto);
        return Result<IEnumerable<ExpenseDto>>.Success(expenseDtos);
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByUserIdAsync(int userId)
    {
        var expenses = await _expenseRepository.GetByUserIdAsync(userId);
        var expenseDtos = expenses.Select(MapToDto);
        return Result<IEnumerable<ExpenseDto>>.Success(expenseDtos);
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByCategoryAsync(ExpenseCategory category)
    {
        var expenses = await _expenseRepository.GetByCategoryAsync(category);
        var expenseDtos = expenses.Select(MapToDto);
        return Result<IEnumerable<ExpenseDto>>.Success(expenseDtos);
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByTagAsync(string tag)
    {
        var expenses = await _expenseRepository.GetByTagAsync(tag);
        var expenseDtos = expenses.Select(MapToDto);
        return Result<IEnumerable<ExpenseDto>>.Success(expenseDtos);
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(startDate, endDate);
        var expenseDtos = expenses.Select(MapToDto);
        return Result<IEnumerable<ExpenseDto>>.Success(expenseDtos);
    }

    public async Task<Result<ExpenseDto>> CreateExpenseAsync(CreateExpenseRequest request)
    {
        try
        {
            var expense = new Expense(
                request.Description,
                request.Amount,
                request.PaidByUserId,
                request.Category,
                request.ExpenseDate);

            if (!string.IsNullOrEmpty(request.Notes))
            {
                expense.SetNotes(request.Notes);
            }

            if (request.Tags != null)
            {
                foreach (var tag in request.Tags)
                {
                    expense.AddTag(tag);
                }
            }

            // Handle splits based on the split type
            if (request.SplitDetails != null)
            {
                switch (request.SplitDetails.SplitType)
                {
                    case SplitType.Equal:
                        if (request.SplitDetails.UserIds != null && request.SplitDetails.UserIds.Any())
                        {
                            expense.SplitEqually(request.SplitDetails.UserIds);
                        }
                        break;

                    case SplitType.Percentage:
                        if (request.SplitDetails.UserPercentages != null && request.SplitDetails.UserPercentages.Any())
                        {
                            expense.SplitByPercentage(request.SplitDetails.UserPercentages);
                        }
                        break;

                    case SplitType.ExactAmount:
                        if (request.SplitDetails.UserAmounts != null && request.SplitDetails.UserAmounts.Any())
                        {
                            expense.SplitByExactAmounts(request.SplitDetails.UserAmounts);
                        }
                        break;

                    case SplitType.Uneven:
                        if (request.SplitDetails.CustomSplits != null && request.SplitDetails.CustomSplits.Any())
                        {
                            var customSplits = request.SplitDetails.CustomSplits.Select(cs =>
                                new ExpenseSplit(
                                    cs.UserId,
                                    cs.Percentage ?? 0,
                                    cs.SplitType,
                                    cs.Amount)
                            ).ToList();

                            expense.SplitUnevenly(customSplits);
                        }
                        break;
                }
            }

            await _expenseRepository.AddAsync(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<ExpenseDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseDto>.Failure($"Failed to create expense: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseDto>> UpdateExpenseAsync(int id, UpdateExpenseRequest request)
    {
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                return Result<ExpenseDto>.Failure("Expense not found");
            }

            expense.UpdateDetails(
                request.Description,
                request.Amount,
                request.Category,
                request.ExpenseDate);

            expense.SetNotes(request.Notes ?? string.Empty);

            // Update tags
            if (request.Tags != null)
            {
                // Clear existing tags and add new ones
                var currentTags = expense.Tags.ToList();
                foreach (var tag in currentTags)
                {
                    expense.RemoveTag(tag);
                }

                foreach (var tag in request.Tags)
                {
                    expense.AddTag(tag);
                }
            }

            // Handle splits based on the split type
            if (request.SplitDetails != null)
            {
                switch (request.SplitDetails.SplitType)
                {
                    case SplitType.Equal:
                        if (request.SplitDetails.UserIds != null && request.SplitDetails.UserIds.Any())
                        {
                            expense.SplitEqually(request.SplitDetails.UserIds);
                        }
                        break;

                    case SplitType.Percentage:
                        if (request.SplitDetails.UserPercentages != null && request.SplitDetails.UserPercentages.Any())
                        {
                            expense.SplitByPercentage(request.SplitDetails.UserPercentages);
                        }
                        break;

                    case SplitType.ExactAmount:
                        if (request.SplitDetails.UserAmounts != null && request.SplitDetails.UserAmounts.Any())
                        {
                            expense.SplitByExactAmounts(request.SplitDetails.UserAmounts);
                        }
                        break;

                    case SplitType.Uneven:
                        if (request.SplitDetails.CustomSplits != null && request.SplitDetails.CustomSplits.Any())
                        {
                            var customSplits = request.SplitDetails.CustomSplits.Select(cs =>
                                new ExpenseSplit(
                                    cs.UserId,
                                    cs.Percentage ?? 0,
                                    cs.SplitType,
                                    cs.Amount)
                            ).ToList();

                            expense.SplitUnevenly(customSplits);
                        }
                        break;
                }
            }

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<ExpenseDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseDto>.Failure($"Failed to update expense: {ex.Message}");
        }
    }

    public async Task<Result> DeleteExpenseAsync(int id)
    {
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                return Result.Failure("Expense not found");
            }

            _expenseRepository.Delete(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete expense: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseDto>> AddAttachmentAsync(int expenseId, AddAttachmentRequest request)
    {
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<ExpenseDto>.Failure("Expense not found");
            }

            expense.AddAttachment(request.FileName, request.FileType, request.FileUrl);

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<ExpenseDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseDto>.Failure($"Failed to add attachment: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseDto>> RemoveAttachmentAsync(int expenseId, int attachmentId)
    {
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<ExpenseDto>.Failure("Expense not found");
            }

            expense.RemoveAttachment(attachmentId);

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<ExpenseDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseDto>.Failure($"Failed to remove attachment: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseDto>> MarkSplitAsPaidAsync(int expenseId, MarkSplitPaidRequest request)
    {
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<ExpenseDto>.Failure("Expense not found");
            }

            expense.MarkSplitAsPaid(request.UserId);

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<ExpenseDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseDto>.Failure($"Failed to mark split as paid: {ex.Message}");
        }
    }

    public async Task<Result<ExpenseDto>> MarkSplitAsUnpaidAsync(int expenseId, MarkSplitPaidRequest request)
    {
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<ExpenseDto>.Failure("Expense not found");
            }

            expense.MarkSplitAsUnpaid(request.UserId);

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<ExpenseDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseDto>.Failure($"Failed to mark split as unpaid: {ex.Message}");
        }
    }

    private static ExpenseDto MapToDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            PaidByUserId = expense.PaidByUserId,
            Category = expense.Category,
            Notes = expense.Notes,
            Tags = expense.Tags.ToList(),
            DefaultSplitType = expense.DefaultSplitType,
            Splits = expense.Splits.Select(s => new ExpenseSplitDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Amount = s.Amount,
                SplitType = s.SplitType,
                Percentage = s.Percentage,
                IsPaid = s.IsPaid,
                PaidDate = s.PaidDate
            }).ToList(),
            Attachments = expense.Attachments.Select(a => new ExpenseAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                FileUrl = a.FileUrl
            }).ToList(),
            ExpenseDate = expense.ExpenseDate,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }
}