using System.ComponentModel.DataAnnotations;
using SharedLibrary.Validation;

namespace AuthService.Application.DTOs;

/// <summary>
/// Request to initiate magic link authentication
/// </summary>
public record MagicLinkRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Request to verify a magic link token
/// </summary>
public record VerifyMagicLinkRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;
}

/// <summary>
/// Request to initiate SMS OTP authentication
/// </summary>
public record SmsOtpRequest
{
    [Required]
    [Phone]
    [NoXss]
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// Request to verify an SMS OTP
/// </summary>
public record VerifySmsOtpRequest
{
    [Required]
    [Phone]
    [NoXss]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; init; } = string.Empty;
}

/// <summary>
/// Request to initiate WebAuthn registration
/// </summary>
public record WebAuthnRegistrationRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    [NoXss]
    public string DeviceName { get; init; } = string.Empty;
}

/// <summary>
/// Response containing WebAuthn registration options
/// </summary>
public record WebAuthnRegistrationOptionsResponse
{
    public string Challenge { get; init; } = string.Empty;
    public string RpId { get; init; } = string.Empty;
    public string RpName { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public int Timeout { get; init; } = 60000; // Default 60 seconds
}

/// <summary>
/// Request to complete WebAuthn registration
/// </summary>
public record CompleteWebAuthnRegistrationRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string CredentialId { get; init; } = string.Empty;

    [Required]
    public string PublicKey { get; init; } = string.Empty;

    [Required]
    public string DeviceName { get; init; } = string.Empty;

    [Required]
    public string Attestation { get; init; } = string.Empty;
}

/// <summary>
/// Request to initiate WebAuthn authentication
/// </summary>
public record WebAuthnAuthenticationRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Response containing WebAuthn authentication options
/// </summary>
public record WebAuthnAuthenticationOptionsResponse
{
    public string Challenge { get; init; } = string.Empty;
    public string RpId { get; init; } = string.Empty;
    public List<WebAuthnCredentialDescriptor> AllowCredentials { get; init; } = new();
    public int Timeout { get; init; } = 60000; // Default 60 seconds
}

/// <summary>
/// WebAuthn credential descriptor
/// </summary>
public record WebAuthnCredentialDescriptor
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = "public-key";
}

/// <summary>
/// Request to complete WebAuthn authentication
/// </summary>
public record CompleteWebAuthnAuthenticationRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string CredentialId { get; init; } = string.Empty;

    [Required]
    public string Signature { get; init; } = string.Empty;

    [Required]
    public string AuthenticatorData { get; init; } = string.Empty;

    [Required]
    public string ClientDataJSON { get; init; } = string.Empty;
}