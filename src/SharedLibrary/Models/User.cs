using System;

namespace SharedLibrary.Models;

/// <summary>
/// Shared User model for use across services
/// This is a simplified version of the User entity from AuthService
/// </summary>
public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public string DeviceToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User()
    {
    }

    public User(string id, string email, string firstName, string lastName)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
    }

    public void UpdateDeviceToken(string deviceToken)
    {
        DeviceToken = deviceToken;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }
}