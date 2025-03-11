namespace SharedLibrary.Interfaces;

/// <summary>
/// Service interface for sending SMS messages
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The recipient's phone number</param>
    /// <param name="message">The message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}