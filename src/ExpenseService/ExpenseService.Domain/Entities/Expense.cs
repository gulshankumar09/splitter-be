using SharedLibrary.Domain;
using System.Collections.Generic;

namespace ExpenseService.Domain.Entities;

public class Expense : BaseEntity
{
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public int PaidByUserId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string? Notes { get; private set; }
    public List<string> Tags { get; private set; }
    public List<ExpenseSplit> Splits { get; private set; }
    public List<ExpenseAttachment> Attachments { get; private set; }
    public DateTime ExpenseDate { get; private set; }
    public SplitType DefaultSplitType { get; private set; }

    // For EF Core
    private Expense() { }

    public Expense(string description, decimal amount, int paidByUserId, ExpenseCategory category, DateTime expenseDate)
        : base()
    {
        Description = description;
        Amount = amount;
        PaidByUserId = paidByUserId;
        Category = category;
        ExpenseDate = expenseDate;
        DefaultSplitType = SplitType.Equal;
        Tags = new List<string>();
        Splits = new List<ExpenseSplit>();
        Attachments = new List<ExpenseAttachment>();
    }

    public void UpdateDetails(string description, decimal amount, ExpenseCategory category, DateTime expenseDate)
    {
        Description = description;
        Amount = amount;
        Category = category;
        ExpenseDate = expenseDate;

        // Recalculate splits if they exist and are not exact amounts
        RecalculateSplits();
    }

    private void RecalculateSplits()
    {
        if (Splits.Count == 0) return;

        switch (DefaultSplitType)
        {
            case SplitType.Equal:
                SplitEqually(Splits.Select(s => s.UserId).ToList());
                break;
            case SplitType.Percentage:
                // Preserve existing percentages
                var percentageSplits = Splits
                    .Where(s => s.SplitType == SplitType.Percentage && s.Percentage.HasValue)
                    .Select(s => new { UserId = s.UserId, Percentage = s.Percentage!.Value })
                    .ToList();

                if (percentageSplits.Any())
                {
                    SplitByPercentage(percentageSplits.ToDictionary(s => s.UserId, s => s.Percentage));
                }
                break;
                // For exact amount and uneven, we don't recalculate automatically
        }
    }

    public void SetNotes(string notes)
    {
        Notes = notes;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
        }
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    public void AddAttachment(string fileName, string fileType, string fileUrl)
    {
        var attachment = new ExpenseAttachment(fileName, fileType, fileUrl);
        Attachments.Add(attachment);
    }

    public void RemoveAttachment(int attachmentId)
    {
        var attachment = Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment != null)
        {
            Attachments.Remove(attachment);
        }
    }

    // Split equally among users
    public void SplitEqually(List<int> userIds)
    {
        if (userIds == null || !userIds.Any())
            return;

        // Clear existing splits
        Splits.Clear();
        DefaultSplitType = SplitType.Equal;

        decimal splitAmount = Math.Round(Amount / userIds.Count, 2);
        decimal remainder = Amount - (splitAmount * userIds.Count);

        for (int i = 0; i < userIds.Count; i++)
        {
            decimal userAmount = splitAmount;

            // Add the remainder to the last user to handle rounding issues
            if (i == userIds.Count - 1 && remainder != 0)
            {
                userAmount += remainder;
            }

            var split = new ExpenseSplit(userIds[i], userAmount, SplitType.Equal);
            Splits.Add(split);
        }
    }

    // Split by percentage
    public void SplitByPercentage(Dictionary<int, decimal> userPercentages)
    {
        if (userPercentages == null || !userPercentages.Any())
            return;

        // Validate that percentages sum to 100
        decimal totalPercentage = userPercentages.Values.Sum();
        if (Math.Abs(totalPercentage - 100) > 0.01m)
            throw new ArgumentException("Percentages must sum to 100");

        // Clear existing splits
        Splits.Clear();
        DefaultSplitType = SplitType.Percentage;

        decimal totalCalculated = 0;
        var lastUserId = userPercentages.Keys.Last();

        foreach (var kvp in userPercentages)
        {
            decimal percentage = kvp.Value;
            decimal calculatedAmount;

            if (kvp.Key == lastUserId)
            {
                // For the last user, use the remaining amount to avoid rounding issues
                calculatedAmount = Amount - totalCalculated;
            }
            else
            {
                calculatedAmount = Math.Round(Amount * percentage / 100, 2);
                totalCalculated += calculatedAmount;
            }

            var split = new ExpenseSplit(kvp.Key, percentage, SplitType.Percentage, calculatedAmount);
            Splits.Add(split);
        }
    }

    // Split by exact amounts
    public void SplitByExactAmounts(Dictionary<int, decimal> userAmounts)
    {
        if (userAmounts == null || !userAmounts.Any())
            return;

        // Validate that amounts sum to the total expense amount
        decimal totalAmount = userAmounts.Values.Sum();
        if (Math.Abs(totalAmount - Amount) > 0.01m)
            throw new ArgumentException("Split amounts must sum to the total expense amount");

        // Clear existing splits
        Splits.Clear();
        DefaultSplitType = SplitType.ExactAmount;

        foreach (var kvp in userAmounts)
        {
            var split = new ExpenseSplit(kvp.Key, kvp.Value, SplitType.ExactAmount);
            Splits.Add(split);
        }
    }

    // Handle uneven splits (custom logic)
    public void SplitUnevenly(List<ExpenseSplit> customSplits)
    {
        if (customSplits == null || !customSplits.Any())
            return;

        // Validate that amounts sum to the total expense amount
        decimal totalAmount = customSplits.Sum(s => s.Amount);
        if (Math.Abs(totalAmount - Amount) > 0.01m)
            throw new ArgumentException("Split amounts must sum to the total expense amount");

        // Clear existing splits
        Splits.Clear();
        DefaultSplitType = SplitType.Uneven;

        foreach (var split in customSplits)
        {
            Splits.Add(split);
        }
    }

    public void MarkSplitAsPaid(int userId)
    {
        var split = Splits.FirstOrDefault(s => s.UserId == userId);
        if (split != null)
        {
            split.MarkAsPaid();
        }
    }

    public void MarkSplitAsUnpaid(int userId)
    {
        var split = Splits.FirstOrDefault(s => s.UserId == userId);
        if (split != null)
        {
            split.MarkAsUnpaid();
        }
    }

    public void AddSplit(int userId, decimal amount)
    {
        var split = new ExpenseSplit(userId, amount);
        Splits.Add(split);
    }

    public void UpdateSplit(int userId, decimal amount)
    {
        var split = Splits.FirstOrDefault(s => s.UserId == userId);
        if (split != null)
        {
            split.UpdateAmount(amount);
        }
    }

    public void RemoveSplit(int userId)
    {
        var split = Splits.FirstOrDefault(s => s.UserId == userId);
        if (split != null)
        {
            Splits.Remove(split);
        }
    }
}