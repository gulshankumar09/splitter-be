using ExpenseService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface ICalendarRepository
{
    // Calendar Connection methods
    Task<CalendarConnection> GetConnectionByIdAsync(int id);
    Task<IEnumerable<CalendarConnection>> GetConnectionsByUserIdAsync(int userId);
    Task<IEnumerable<CalendarConnection>> GetActiveConnectionsAsync();
    Task AddConnectionAsync(CalendarConnection connection);
    void UpdateConnection(CalendarConnection connection);
    Task<bool> DeleteConnectionAsync(int id);

    // Calendar Event methods
    Task<CalendarEvent> GetEventByIdAsync(int id);
    Task<CalendarEvent> GetEventByExternalIdAsync(string externalId, int connectionId);
    Task<IEnumerable<CalendarEvent>> GetEventsByConnectionIdAsync(int connectionId);
    Task<IEnumerable<CalendarEvent>> GetEventsByTypeAndEntityIdAsync(CalendarEventType eventType, int entityId);
    Task AddEventAsync(CalendarEvent calendarEvent);
    void UpdateEvent(CalendarEvent calendarEvent);
    Task<bool> DeleteEventAsync(int id);

    Task SaveChangesAsync();
}