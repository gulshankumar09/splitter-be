using SharedLibrary.Models;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

/// <summary>
/// Client interface for communicating with the AuthService
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>
    /// Gets a user by their ID from the AuthService
    /// </summary>
    Task<Result<User>> GetUserByIdAsync(string userId);
    
    /// <summary>
    /// Gets a user by their email from the AuthService
    /// </summary>
    Task<Result<User>> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// Validates a JWT token with the AuthService
    /// </summary>
    Task<Result<bool>> ValidateTokenAsync(string token);
} 