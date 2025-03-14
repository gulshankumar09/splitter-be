using SharedLibrary.Domain;
using System;

namespace ExpenseService.Domain.Entities;

public class CalendarConnection : BaseEntity
{
    public int UserId { get; private set; }
    public string CalendarProviderId { get; private set; }
    public string AccountName { get; private set; }
    public string CalendarId { get; private set; }
    public string CalendarName { get; private set; }
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public DateTime TokenExpiryDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime LastSyncedAt { get; private set; }

    // For EF Core
    private CalendarConnection() { }

    public CalendarConnection(
        int userId,
        string calendarProviderId,
        string accountName,
        string calendarId,
        string calendarName,
        string accessToken,
        string refreshToken,
        DateTime tokenExpiryDate)
    {
        UserId = userId;
        CalendarProviderId = calendarProviderId;
        AccountName = accountName;
        CalendarId = calendarId;
        CalendarName = calendarName;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiryDate = tokenExpiryDate;
        IsActive = true;
        LastSyncedAt = DateTime.UtcNow;
    }

    public void UpdateTokens(string accessToken, string refreshToken, DateTime tokenExpiryDate)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiryDate = tokenExpiryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCalendarInfo(string calendarName)
    {
        CalendarName = calendarName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastSyncedTime()
    {
        LastSyncedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool NeedsTokenRefresh()
    {
        // Allow 5 minutes buffer before token actually expires
        return DateTime.UtcNow.AddMinutes(5) >= TokenExpiryDate;
    }
}