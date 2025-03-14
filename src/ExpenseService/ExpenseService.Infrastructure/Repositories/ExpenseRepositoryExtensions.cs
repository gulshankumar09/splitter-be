using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories;

public static class ExpenseRepositoryExtensions
{
    public static async Task<Dictionary<ExpenseCategory, decimal>> GetCategoryTotalsAsync(
        this IQueryable<Expense> expenses, DateTime startDate, DateTime endDate)
    {
        return await expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Category, x => x.Total);
    }

    public static async Task<Dictionary<DateTime, decimal>> GetMonthlyTotalsAsync(
        this IQueryable<Expense> expenses, DateTime startDate, DateTime endDate)
    {
        return await expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .GroupBy(e => new DateTime(e.ExpenseDate.Year, e.ExpenseDate.Month, 1))
            .Select(g => new { Month = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Month, x => x.Total);
    }

    public static async Task<Dictionary<int, decimal>> GetUserTotalsAsync(
        this IQueryable<Expense> expenses, DateTime startDate, DateTime endDate)
    {
        return await expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .GroupBy(e => e.PaidByUserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total);
    }
}