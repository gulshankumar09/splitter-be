using ExpenseService.Application.DTOs;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface ICalendarIntegrationService
{
    /// <summary>
    /// Connects a user's calendar for expense planning
    /// </summary>
    Task<Result<CalendarConnectionDto>> ConnectCalendarAsync(ConnectCalendarRequest request);

    /// <summary>
    /// Gets a list of connected calendars for a user
    /// </summary>
    Task<Result<IEnumerable<CalendarConnectionDto>>> GetConnectedCalendarsAsync(int userId);

    /// <summary>
    /// Disconnects a calendar
    /// </summary>
    Task<Result<bool>> DisconnectCalendarAsync(int connectionId);

    /// <summary>
    /// Creates an event in the user's calendar for a recurring expense
    /// </summary>
    Task<Result<CalendarEventDto>> CreateRecurringExpenseEventAsync(int recurringExpenseId, int calendarConnectionId);

    /// <summary>
    /// Creates an event in the user's calendar for a budget deadline
    /// </summary>
    Task<Result<CalendarEventDto>> CreateBudgetDeadlineEventAsync(int budgetId, int calendarConnectionId);

    /// <summary>
    /// Creates a reminder event for payment due
    /// </summary>
    Task<Result<CalendarEventDto>> CreatePaymentReminderEventAsync(CreatePaymentReminderRequest request);

    /// <summary>
    /// Updates an existing calendar event
    /// </summary>
    Task<Result<CalendarEventDto>> UpdateCalendarEventAsync(UpdateCalendarEventRequest request);

    /// <summary>
    /// Deletes a calendar event
    /// </summary>
    Task<Result<bool>> DeleteCalendarEventAsync(string eventId, int calendarConnectionId);

    /// <summary>
    /// Syncs all expense-related events to the user's calendar
    /// </summary>
    Task<Result<CalendarSyncResultDto>> SyncCalendarEventsAsync(int userId);

    /// <summary>
    /// Gets a list of available calendar providers
    /// </summary>
    Task<Result<IEnumerable<CalendarProviderDto>>> GetAvailableCalendarProvidersAsync();
}