using SharedLibrary.Domain;
using System;
using System.Collections.Generic;

namespace ExpenseService.Domain.Entities;

public class CalendarEvent : BaseEntity
{
    public int CalendarConnectionId { get; private set; }
    public string ExternalEventId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsAllDay { get; private set; }
    public bool IsRecurring { get; private set; }
    public string RecurrenceRule { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }
    public string EventUrl { get; private set; }
    public CalendarEventType EventType { get; private set; }
    public int? RelatedEntityId { get; private set; }

    // For EF Core
    private CalendarEvent() { }

    public CalendarEvent(
        int calendarConnectionId,
        string externalEventId,
        string title,
        string description,
        DateTime startTime,
        DateTime endTime,
        CalendarEventType eventType,
        int? relatedEntityId = null,
        string location = null,
        bool isAllDay = false,
        bool isRecurring = false,
        string recurrenceRule = null,
        Dictionary<string, string> metadata = null,
        string eventUrl = null)
    {
        CalendarConnectionId = calendarConnectionId;
        ExternalEventId = externalEventId;
        Title = title;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        EventType = eventType;
        RelatedEntityId = relatedEntityId;
        Location = location;
        IsAllDay = isAllDay;
        IsRecurring = isRecurring;
        RecurrenceRule = recurrenceRule;
        Metadata = metadata ?? new Dictionary<string, string>();
        EventUrl = eventUrl;
    }

    public void UpdateDetails(
        string title = null,
        string description = null,
        string location = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        bool? isAllDay = null,
        bool? isRecurring = null,
        string recurrenceRule = null,
        Dictionary<string, string> metadata = null,
        string eventUrl = null)
    {
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (location != null) Location = location;
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
        if (isAllDay.HasValue) IsAllDay = isAllDay.Value;
        if (isRecurring.HasValue) IsRecurring = isRecurring.Value;
        if (recurrenceRule != null) RecurrenceRule = recurrenceRule;
        if (metadata != null) Metadata = metadata;
        if (eventUrl != null) EventUrl = eventUrl;

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExternalEventId(string externalEventId)
    {
        ExternalEventId = externalEventId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum CalendarEventType
{
    RecurringExpense,
    BudgetDeadline,
    PaymentReminder,
    SettlementDue
}