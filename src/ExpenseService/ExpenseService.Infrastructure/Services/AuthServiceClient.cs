using ExpenseService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Services;

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthServiceClient> _logger;
    private readonly AuthServiceOptions _options;

    public AuthServiceClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthServiceOptions> options,
        ILogger<AuthServiceClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("AuthService");
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<User>> GetUserByIdAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting user by ID {UserId}: {ErrorContent}", userId, errorContent);
                return Result<User>.Failure($"Failed to get user: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<Result<User>>();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting user by ID {UserId}", userId);
            return Result<User>.Failure($"Exception getting user: {ex.Message}");
        }
    }

    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/by-email/{email}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting user by email {Email}: {ErrorContent}", email, errorContent);
                return Result<User>.Failure($"Failed to get user: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<Result<User>>();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting user by email {Email}", email);
            return Result<User>.Failure($"Exception getting user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("api/auth/validate-token");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error validating token: {ErrorContent}", errorContent);
                return Result<bool>.Failure($"Failed to validate token: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception validating token");
            return Result<bool>.Failure($"Exception validating token: {ex.Message}");
        }
    }
}

public class AuthServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5001";
    public int TimeoutSeconds { get; set; } = 30;
}