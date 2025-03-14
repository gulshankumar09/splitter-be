using SharedLibrary.Domain;
using System;
using System.Collections.Generic;

namespace ExpenseService.Domain.Entities;

public class Budget : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int UserId { get; private set; }
    public int? GroupId { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; }
    public BudgetPeriod Period { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsRecurring { get; private set; }
    public bool IsActive { get; private set; }
    public List<BudgetCategory> Categories { get; private set; }
    public BudgetNotificationThreshold? WarningThreshold { get; private set; }
    public bool NotificationsEnabled { get; private set; }

    // For EF Core
    private Budget()
    {
        Categories = new List<BudgetCategory>();
    }

    public Budget(
        string name,
        string description,
        int userId,
        decimal amount,
        string currencyCode,
        BudgetPeriod period,
        DateTime startDate,
        DateTime endDate,
        bool isRecurring = false,
        int? groupId = null)
    {
        Name = name;
        Description = description;
        UserId = userId;
        GroupId = groupId;
        Amount = amount;
        CurrencyCode = currencyCode;
        Period = period;
        StartDate = startDate;
        EndDate = endDate;
        IsRecurring = isRecurring;
        IsActive = true;
        Categories = new List<BudgetCategory>();
        NotificationsEnabled = true;
    }

    public void Update(
        string name,
        string description,
        decimal amount,
        string currencyCode,
        BudgetPeriod period,
        DateTime startDate,
        DateTime endDate,
        bool isRecurring,
        bool isActive)
    {
        Name = name;
        Description = description;
        Amount = amount;
        CurrencyCode = currencyCode;
        Period = period;
        StartDate = startDate;
        EndDate = endDate;
        IsRecurring = isRecurring;
        IsActive = isActive;
    }

    public void AddCategory(ExpenseCategory category, decimal? limitAmount = null)
    {
        // Check if category already exists
        if (Categories.Any(c => c.Category == category))
        {
            throw new InvalidOperationException($"Category {category} already exists in this budget.");
        }

        var budgetCategory = new BudgetCategory(category, limitAmount);
        Categories.Add(budgetCategory);
    }

    public void UpdateCategoryLimit(ExpenseCategory category, decimal? limitAmount)
    {
        var budgetCategory = Categories.FirstOrDefault(c => c.Category == category);
        if (budgetCategory == null)
        {
            throw new KeyNotFoundException($"Category {category} not found in this budget.");
        }

        budgetCategory.UpdateLimit(limitAmount);
    }

    public void RemoveCategory(ExpenseCategory category)
    {
        var budgetCategory = Categories.FirstOrDefault(c => c.Category == category);
        if (budgetCategory == null)
        {
            throw new KeyNotFoundException($"Category {category} not found in this budget.");
        }

        Categories.Remove(budgetCategory);
    }

    public void SetNotificationThreshold(BudgetNotificationThreshold threshold)
    {
        WarningThreshold = threshold;
    }

    public void EnableNotifications(bool enabled)
    {
        NotificationsEnabled = enabled;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public DateTime GetNextPeriodStartDate()
    {
        if (!IsRecurring)
        {
            return StartDate;
        }

        return Period switch
        {
            BudgetPeriod.Daily => EndDate.AddDays(1),
            BudgetPeriod.Weekly => EndDate.AddDays(7),
            BudgetPeriod.Monthly => EndDate.AddMonths(1),
            BudgetPeriod.Quarterly => EndDate.AddMonths(3),
            BudgetPeriod.Yearly => EndDate.AddYears(1),
            _ => EndDate
        };
    }
}