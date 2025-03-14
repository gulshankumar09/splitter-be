using System;
using System.Collections.Generic;

namespace ExpenseService.Application.DTOs;

public class CalendarProviderDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public bool RequiresOAuth { get; set; }
    public string AuthUrl { get; set; }
}

public class CalendarConnectionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CalendarProviderId { get; set; }
    public string ProviderName { get; set; }
    public string AccountName { get; set; }
    public string CalendarName { get; set; }
    public string CalendarId { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CalendarEventDto
{
    public string EventId { get; set; }
    public int CalendarConnectionId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsRecurring { get; set; }
    public string RecurrenceRule { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public string EventUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ConnectCalendarRequest
{
    public int UserId { get; set; }
    public string CalendarProviderId { get; set; }
    public string AuthToken { get; set; }
    public string RefreshToken { get; set; }
    public string CalendarId { get; set; }
    public string CalendarName { get; set; }
}

public class CreatePaymentReminderRequest
{
    public int UserId { get; set; }
    public int CalendarConnectionId { get; set; }
    public int? ExpenseId { get; set; }
    public int? SettlementId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public int ReminderDaysBefore { get; set; } = 1;
}

public class UpdateCalendarEventRequest
{
    public string EventId { get; set; }
    public int CalendarConnectionId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsAllDay { get; set; }
    public bool? IsRecurring { get; set; }
    public string RecurrenceRule { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public class CalendarSyncResultDto
{
    public int UserId { get; set; }
    public List<CalendarEventDto> CreatedEvents { get; set; } = new List<CalendarEventDto>();
    public List<CalendarEventDto> UpdatedEvents { get; set; } = new List<CalendarEventDto>();
    public List<string> DeletedEventIds { get; set; } = new List<string>();
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}