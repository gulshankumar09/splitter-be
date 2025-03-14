using SharedLibrary.Domain;
using System;
using System.Collections.Generic;

namespace ExpenseService.Domain.Entities;

public class RecurringExpense : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public int CreatedByUserId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public RecurrenceFrequency Frequency { get; private set; }
    public int? FrequencyParam { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime NextOccurrence { get; private set; }
    public DateTime? LastProcessed { get; private set; }
    public bool IsActive { get; private set; }
    public List<int> ParticipantUserIds { get; private set; }
    public int? GroupId { get; private set; }
    public SplitType SplitType { get; private set; }
    public bool SendReminders { get; private set; }
    public int ReminderDaysBefore { get; private set; }

    // For EF Core
    private RecurringExpense() { }

    public RecurringExpense(
        string title,
        string description,
        decimal amount,
        int createdByUserId,
        ExpenseCategory category,
        RecurrenceFrequency frequency,
        DateTime startDate,
        List<int> participantUserIds,
        SplitType splitType = SplitType.Equal,
        int? frequencyParam = null,
        DateTime? endDate = null,
        int? groupId = null)
    {
        Title = title;
        Description = description;
        Amount = amount;
        CreatedByUserId = createdByUserId;
        Category = category;
        Frequency = frequency;
        FrequencyParam = frequencyParam;
        StartDate = startDate;
        EndDate = endDate;
        NextOccurrence = CalculateNextOccurrence(startDate);
        ParticipantUserIds = participantUserIds ?? new List<int>();
        GroupId = groupId;
        SplitType = splitType;
        IsActive = true;
        SendReminders = true;
        ReminderDaysBefore = 3;
    }

    public void UpdateDetails(
        string title,
        string description,
        decimal amount,
        ExpenseCategory category,
        RecurrenceFrequency frequency,
        DateTime startDate,
        List<int> participantUserIds,
        SplitType splitType,
        int? frequencyParam = null,
        DateTime? endDate = null,
        int? groupId = null)
    {
        Title = title;
        Description = description;
        Amount = amount;
        Category = category;
        Frequency = frequency;
        FrequencyParam = frequencyParam;

        // Only update start date and next occurrence if the start date is in the future
        if (startDate > DateTime.Today)
        {
            StartDate = startDate;
            NextOccurrence = CalculateNextOccurrence(startDate);
        }

        EndDate = endDate;
        ParticipantUserIds = participantUserIds ?? new List<int>();
        GroupId = groupId;
        SplitType = splitType;
    }

    public void UpdateNotificationSettings(bool sendReminders, int reminderDaysBefore)
    {
        SendReminders = sendReminders;
        ReminderDaysBefore = reminderDaysBefore;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public Expense GenerateExpense()
    {
        var expense = new Expense(
            Description,
            Amount,
            CreatedByUserId,
            Category,
            NextOccurrence);

        // Apply split logic based on split type and participants
        if (ParticipantUserIds.Count > 0)
        {
            switch (SplitType)
            {
                case SplitType.Equal:
                    expense.SplitEqually(ParticipantUserIds);
                    break;

                    // Add other split types as needed
                    // For simplicity, we'll default to equal splits for recurring expenses
            }
        }

        // If it's associated with a group, set the group ID
        if (GroupId.HasValue)
        {
            expense.AssignToGroup(GroupId.Value);
        }

        // Update the last processed date and calculate the next occurrence
        LastProcessed = NextOccurrence;
        NextOccurrence = CalculateNextOccurrence(NextOccurrence);

        return expense;
    }

    public DateTime CalculateNextOccurrence(DateTime fromDate)
    {
        var next = fromDate;

        switch (Frequency)
        {
            case RecurrenceFrequency.Daily:
                next = next.AddDays(FrequencyParam ?? 1);
                break;

            case RecurrenceFrequency.Weekly:
                next = next.AddDays((FrequencyParam ?? 1) * 7);
                break;

            case RecurrenceFrequency.Monthly:
                next = next.AddMonths(FrequencyParam ?? 1);
                break;

            case RecurrenceFrequency.Yearly:
                next = next.AddYears(FrequencyParam ?? 1);
                break;

            case RecurrenceFrequency.Custom:
                // For custom frequency, FrequencyParam is required and represents days
                if (FrequencyParam.HasValue)
                {
                    next = next.AddDays(FrequencyParam.Value);
                }
                else
                {
                    next = next.AddMonths(1); // Default to monthly if no param provided
                }
                break;
        }

        // If we've reached or passed the end date, return max value to indicate no more occurrences
        if (EndDate.HasValue && next > EndDate.Value)
        {
            Deactivate();
            return DateTime.MaxValue;
        }

        return next;
    }

    public bool ShouldSendReminder()
    {
        if (!SendReminders || !IsActive)
        {
            return false;
        }

        var reminderDate = NextOccurrence.AddDays(-ReminderDaysBefore);
        return DateTime.Today >= reminderDate && DateTime.Today < NextOccurrence;
    }
}

public enum RecurrenceFrequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly,
    Custom
}