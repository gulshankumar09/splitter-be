using AuthService.Application.DTOs;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for passwordless authentication operations
/// </summary>
public interface IPasswordlessAuthService
{
    /// <summary>
    /// Initiates magic link authentication by sending a link to the user's email
    /// </summary>
    Task<IResult> SendMagicLinkAsync(MagicLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a magic link token and authenticates the user
    /// </summary>
    Task<IResult<AuthResponse>> VerifyMagicLinkAsync(VerifyMagicLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates SMS OTP authentication by sending a code to the user's phone
    /// </summary>
    Task<IResult> SendSmsOtpAsync(SmsOtpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies an SMS OTP and authenticates the user
    /// </summary>
    Task<IResult<AuthResponse>> VerifySmsOtpAsync(VerifySmsOtpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates WebAuthn registration for a user
    /// </summary>
    Task<IResult<WebAuthnRegistrationOptionsResponse>> InitiateWebAuthnRegistrationAsync(WebAuthnRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes WebAuthn registration for a user
    /// </summary>
    Task<IResult> CompleteWebAuthnRegistrationAsync(CompleteWebAuthnRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates WebAuthn authentication for a user
    /// </summary>
    Task<IResult<WebAuthnAuthenticationOptionsResponse>> InitiateWebAuthnAuthenticationAsync(WebAuthnAuthenticationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes WebAuthn authentication for a user
    /// </summary>
    Task<IResult<AuthResponse>> CompleteWebAuthnAuthenticationAsync(CompleteWebAuthnAuthenticationRequest request, CancellationToken cancellationToken = default);
}