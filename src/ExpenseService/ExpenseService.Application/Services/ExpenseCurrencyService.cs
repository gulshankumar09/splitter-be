using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ExpenseService.Application.Services
{
    public class ExpenseCurrencyService : IExpenseCurrencyService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICurrencyConversionService _currencyConversionService;
        private readonly ICurrencyService _currencyService;
        private readonly IGroupRepository _groupRepository;

        public ExpenseCurrencyService(
            IExpenseRepository expenseRepository,
            ICurrencyConversionService currencyConversionService,
            ICurrencyService currencyService,
            IGroupRepository groupRepository)
        {
            _expenseRepository = expenseRepository;
            _currencyConversionService = currencyConversionService;
            _currencyService = currencyService;
            _groupRepository = groupRepository;
        }

        public async Task<ExpenseDto> ConvertExpenseCurrencyAsync(int expenseId, string toCurrencyCode)
        {
            var expense = await _expenseRepository.GetByIdWithDetailsAsync(expenseId);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with ID {expenseId} not found");
            }

            if (string.Equals(expense.CurrencyCode, toCurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                // Already in the requested currency
                return MapToDto(expense);
            }

            // Get the exchange rate
            var rate = await _currencyConversionService.GetExchangeRateAsync(
                expense.CurrencyCode,
                toCurrencyCode);

            // Use the ConvertCurrency method instead of direct property assignment
            expense.ConvertCurrency(toCurrencyCode, rate);

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return MapToDto(expense);
        }

        public async Task<ExpenseDto> ConvertExpenseToUserCurrencyAsync(int expenseId, int userId)
        {
            var userPreference = await _currencyService.GetUserPreferenceAsync(userId);
            return await ConvertExpenseCurrencyAsync(expenseId, userPreference.DefaultCurrencyCode);
        }

        public async Task<ExpenseDto> ConvertExpenseToGroupCurrencyAsync(int expenseId, int groupId)
        {
            var groupPreference = await _currencyService.GetGroupPreferenceAsync(groupId);
            return await ConvertExpenseCurrencyAsync(expenseId, groupPreference.DefaultCurrencyCode);
        }

        public async Task ConvertAllGroupExpensesToGroupCurrencyAsync(int groupId)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                throw new KeyNotFoundException($"Group with ID {groupId} not found");
            }

            var groupPreference = await _currencyService.GetGroupPreferenceAsync(groupId);
            var groupExpenses = await _expenseRepository.GetByGroupIdAsync(groupId);

            foreach (var expense in groupExpenses)
            {
                if (!string.Equals(expense.CurrencyCode, groupPreference.DefaultCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    await ConvertExpenseCurrencyAsync(expense.Id, groupPreference.DefaultCurrencyCode);
                }
            }
        }

        public async Task<string> GetDefaultExpenseCurrencyAsync(int userId, int? groupId = null)
        {
            if (groupId.HasValue)
            {
                var groupPreference = await _currencyService.GetGroupPreferenceAsync(groupId.Value);
                if (groupPreference.AutoConvertForMembers)
                {
                    return groupPreference.DefaultCurrencyCode;
                }
            }

            var userPreference = await _currencyService.GetUserPreferenceAsync(userId);
            return userPreference.DefaultCurrencyCode;
        }

        private static ExpenseDto MapToDto(Expense expense)
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
                Tags = expense.Tags.ToList(),
                Splits = expense.Splits?.Select(s => new ExpenseSplitDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    Amount = s.Amount,
                    SplitType = s.SplitType,
                    Percentage = s.Percentage,
                    IsPaid = s.IsPaid,
                    PaidDate = s.PaidDate
                }).ToList(),
                Attachments = expense.Attachments?.Select(a => new ExpenseAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileUrl = a.FileUrl
                }).ToList(),
                ExpenseDate = expense.ExpenseDate,
                DefaultSplitType = expense.DefaultSplitType,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };
        }
    }
}