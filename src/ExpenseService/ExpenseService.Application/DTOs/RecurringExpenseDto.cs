using ExpenseService.Domain.Entities;
using System;
using System.Collections.Generic;

namespace ExpenseService.Application.DTOs;

public class RecurringExpenseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CreatedByUserId { get; set; }
    public ExpenseCategory Category { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int? FrequencyParam { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime NextOccurrence { get; set; }
    public DateTime? LastProcessed { get; set; }
    public bool IsActive { get; set; }
    public List<int> ParticipantUserIds { get; set; } = new();
    public int? GroupId { get; set; }
    public SplitType SplitType { get; set; }
    public bool SendReminders { get; set; }
    public int ReminderDaysBefore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRecurringExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CreatedByUserId { get; set; }
    public ExpenseCategory Category { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int? FrequencyParam { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<int> ParticipantUserIds { get; set; } = new();
    public int? GroupId { get; set; }
    public SplitType SplitType { get; set; } = SplitType.Equal;
    public bool SendReminders { get; set; } = true;
    public int ReminderDaysBefore { get; set; } = 3;
}

public class UpdateRecurringExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int? FrequencyParam { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<int> ParticipantUserIds { get; set; } = new();
    public int? GroupId { get; set; }
    public SplitType SplitType { get; set; } = SplitType.Equal;
}

public class UpdateNotificationSettingsRequest
{
    public bool SendReminders { get; set; }
    public int ReminderDaysBefore { get; set; }
}

public class RecurringExpenseReminderDto
{
    public int RecurringExpenseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime NextOccurrenceDate { get; set; }
    public int DaysRemaining { get; set; }
}