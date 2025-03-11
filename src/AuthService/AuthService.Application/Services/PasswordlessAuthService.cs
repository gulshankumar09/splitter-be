using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Application.Services;

/// <summary>
/// Service for handling passwordless authentication operations
/// </summary>
public class PasswordlessAuthService : IPasswordlessAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordlessAuthService> _logger;
    private readonly IAuthService _authService;

    private const int MAGIC_LINK_EXPIRY_MINUTES = 15;
    private const int SMS_OTP_EXPIRY_MINUTES = 10;
    private const int OTP_LENGTH = 6;

    public PasswordlessAuthService(
        UserManager<User> userManager,
        IEmailService emailService,
        ISmsService smsService,
        IConfiguration configuration,
        ILogger<PasswordlessAuthService> logger,
        IAuthService authService)
    {
        _userManager = userManager;
        _emailService = emailService;
        _smsService = smsService;
        _configuration = configuration;
        _logger = logger;
        _authService = authService;
    }

    /// <inheritdoc/>
    public async Task<IResult> SendMagicLinkAsync(MagicLinkRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            // Generate a secure random token
            var token = GenerateSecureToken();

            // Set the token in the user entity
            user.SetMagicLinkToken(token, TimeSpan.FromMinutes(MAGIC_LINK_EXPIRY_MINUTES));

            // Save the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure("Failed to generate magic link.");
            }

            // Create the magic link URL
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://yourdomain.com";
            var magicLinkUrl = $"{baseUrl}/auth/verify-magic-link?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";

            // Send the email with the magic link
            await _emailService.SendEmailAsync(
                request.Email,
                "Your Magic Link for Login",
                $"<p>Click the link below to log in:</p><p><a href=\"{magicLinkUrl}\">Login Now</a></p><p>This link will expire in {MAGIC_LINK_EXPIRY_MINUTES} minutes.</p>");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending magic link for {Email}", request.Email);
            return Result.Failure("An error occurred while sending the magic link.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<AuthResponse>> VerifyMagicLinkAsync(VerifyMagicLinkRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<AuthResponse>.Failure("User not found.");
            }

            // Validate the magic link token
            if (!user.ValidateMagicLinkToken(request.Token))
            {
                return Result<AuthResponse>.Failure("Invalid or expired magic link.");
            }

            // Clear the token to prevent reuse
            user.ClearMagicLinkToken();
            await _userManager.UpdateAsync(user);

            // Generate authentication response
            var authResponse = await _authService.LoginAsync(new LoginRequest { Email = user.Email });
            if (!authResponse.IsSuccess)
            {
                return Result<AuthResponse>.Failure("Failed to authenticate user.");
            }

            return Result<AuthResponse>.Success(authResponse.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying magic link for {Email}", request.Email);
            return Result<AuthResponse>.Failure("An error occurred while verifying the magic link.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult> SendSmsOtpAsync(SmsOtpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by phone number
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            // Generate a random OTP
            var otp = GenerateNumericOtp(OTP_LENGTH);

            // Set the OTP in the user entity
            user.SetSmsOtpToken(otp, TimeSpan.FromMinutes(SMS_OTP_EXPIRY_MINUTES));

            // Save the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure("Failed to generate OTP.");
            }

            // Send the SMS with the OTP
            await _smsService.SendSmsAsync(
                request.PhoneNumber,
                $"Your login code is: {otp}. It will expire in {SMS_OTP_EXPIRY_MINUTES} minutes.");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS OTP for {PhoneNumber}", request.PhoneNumber);
            return Result.Failure("An error occurred while sending the OTP.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<AuthResponse>> VerifySmsOtpAsync(VerifySmsOtpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by phone number
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (user == null)
            {
                return Result<AuthResponse>.Failure("User not found.");
            }

            // Validate the OTP
            if (!user.ValidateSmsOtpToken(request.Otp))
            {
                return Result<AuthResponse>.Failure("Invalid or expired OTP.");
            }

            // Clear the OTP to prevent reuse
            user.ClearSmsOtpToken();
            await _userManager.UpdateAsync(user);

            // Generate authentication response
            var authResponse = await _authService.LoginAsync(new LoginRequest { Email = user.Email });
            if (!authResponse.IsSuccess)
            {
                return Result<AuthResponse>.Failure("Failed to authenticate user.");
            }

            return Result<AuthResponse>.Success(authResponse.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying SMS OTP for {PhoneNumber}", request.PhoneNumber);
            return Result<AuthResponse>.Failure("An error occurred while verifying the OTP.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<WebAuthnRegistrationOptionsResponse>> InitiateWebAuthnRegistrationAsync(WebAuthnRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<WebAuthnRegistrationOptionsResponse>.Failure("User not found.");
            }

            // Generate a random challenge
            var challenge = GenerateSecureToken();

            // Store the challenge in session or cache for later verification
            // This would typically be handled by a WebAuthn library

            var rpId = _configuration["WebAuthn:RelyingPartyId"] ?? "yourdomain.com";
            var rpName = _configuration["WebAuthn:RelyingPartyName"] ?? "Your Application";

            var options = new WebAuthnRegistrationOptionsResponse
            {
                Challenge = challenge,
                RpId = rpId,
                RpName = rpName,
                UserId = user.Id,
                UserName = user.UserName ?? user.Email,
                Timeout = 60000 // 60 seconds
            };

            return Result<WebAuthnRegistrationOptionsResponse>.Success(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating WebAuthn registration for {Email}", request.Email);
            return Result<WebAuthnRegistrationOptionsResponse>.Failure("An error occurred while initiating WebAuthn registration.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult> CompleteWebAuthnRegistrationAsync(CompleteWebAuthnRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            // In a real implementation, you would:
            // 1. Verify the attestation using a WebAuthn library
            // 2. Validate the challenge that was stored during initiation
            // 3. Verify the origin and other security parameters

            // For this example, we'll assume the verification was successful
            user.RegisterWebAuthnCredential(request.CredentialId, request.PublicKey, request.DeviceName);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure("Failed to register WebAuthn credential.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing WebAuthn registration for {Email}", request.Email);
            return Result.Failure("An error occurred while completing WebAuthn registration.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<WebAuthnAuthenticationOptionsResponse>> InitiateWebAuthnAuthenticationAsync(WebAuthnAuthenticationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<WebAuthnAuthenticationOptionsResponse>.Failure("User not found.");
            }

            if (!user.HasWebAuthnCredential())
            {
                return Result<WebAuthnAuthenticationOptionsResponse>.Failure("No WebAuthn credential found for this user.");
            }

            // Generate a random challenge
            var challenge = GenerateSecureToken();

            // Store the challenge in session or cache for later verification
            // This would typically be handled by a WebAuthn library

            var rpId = _configuration["WebAuthn:RelyingPartyId"] ?? "yourdomain.com";

            var options = new WebAuthnAuthenticationOptionsResponse
            {
                Challenge = challenge,
                RpId = rpId,
                AllowCredentials = new List<WebAuthnCredentialDescriptor>
                {
                    new WebAuthnCredentialDescriptor
                    {
                        Id = user.WebAuthnCredentialId!
                    }
                },
                Timeout = 60000 // 60 seconds
            };

            return Result<WebAuthnAuthenticationOptionsResponse>.Success(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating WebAuthn authentication for {Email}", request.Email);
            return Result<WebAuthnAuthenticationOptionsResponse>.Failure("An error occurred while initiating WebAuthn authentication.");
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<AuthResponse>> CompleteWebAuthnAuthenticationAsync(CompleteWebAuthnAuthenticationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<AuthResponse>.Failure("User not found.");
            }

            if (!user.HasWebAuthnCredential() || user.WebAuthnCredentialId != request.CredentialId)
            {
                return Result<AuthResponse>.Failure("Invalid credential.");
            }

            // In a real implementation, you would:
            // 1. Verify the signature using the stored public key
            // 2. Validate the challenge that was stored during initiation
            // 3. Verify the authenticator data and client data
            // 4. Check the counter to prevent replay attacks

            // For this example, we'll assume the verification was successful

            // Generate authentication response
            var authResponse = await _authService.LoginAsync(new LoginRequest { Email = user.Email });
            if (!authResponse.IsSuccess)
            {
                return Result<AuthResponse>.Failure("Failed to authenticate user.");
            }

            return Result<AuthResponse>.Success(authResponse.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing WebAuthn authentication for {Email}", request.Email);
            return Result<AuthResponse>.Failure("An error occurred while completing WebAuthn authentication.");
        }
    }

    #region Helper Methods

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateNumericOtp(int length)
    {
        var random = new Random();
        var otp = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            otp.Append(random.Next(0, 10));
        }
        return otp.ToString();
    }

    #endregion
}