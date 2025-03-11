using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace AuthService.API.Controllers;

/// <summary>
/// Controller for handling passwordless authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PasswordlessAuthController : ControllerBase
{
    private readonly IPasswordlessAuthService _passwordlessAuthService;
    private readonly ILogger<PasswordlessAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the PasswordlessAuthController
    /// </summary>
    /// <param name="passwordlessAuthService">The passwordless authentication service</param>
    /// <param name="logger">The logger instance</param>
    public PasswordlessAuthController(
        IPasswordlessAuthService passwordlessAuthService,
        ILogger<PasswordlessAuthController> logger)
    {
        _passwordlessAuthService = passwordlessAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a magic link to the user's email for passwordless authentication
    /// </summary>
    /// <param name="request">The magic link request containing the email</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A success message if the magic link is sent successfully</returns>
    /// <response code="200">Returns success message when magic link is sent</response>
    /// <response code="400">Returns error message when sending magic link fails</response>
    [HttpPost("magic-link/send")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> SendMagicLink(
        [FromBody] MagicLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.SendMagicLinkAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Verifies a magic link token and authenticates the user
    /// </summary>
    /// <param name="request">The verification request containing the email and token</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when verification is successful</response>
    /// <response code="400">Returns error message when verification fails</response>
    [HttpPost("magic-link/verify")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> VerifyMagicLink(
        [FromBody] VerifyMagicLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.VerifyMagicLinkAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Sends an SMS OTP to the user's phone for passwordless authentication
    /// </summary>
    /// <param name="request">The SMS OTP request containing the phone number</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A success message if the OTP is sent successfully</returns>
    /// <response code="200">Returns success message when OTP is sent</response>
    /// <response code="400">Returns error message when sending OTP fails</response>
    [HttpPost("sms-otp/send")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> SendSmsOtp(
        [FromBody] SmsOtpRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.SendSmsOtpAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Verifies an SMS OTP and authenticates the user
    /// </summary>
    /// <param name="request">The verification request containing the phone number and OTP</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when verification is successful</response>
    /// <response code="400">Returns error message when verification fails</response>
    [HttpPost("sms-otp/verify")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> VerifySmsOtp(
        [FromBody] VerifySmsOtpRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.VerifySmsOtpAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Initiates WebAuthn registration for a user
    /// </summary>
    /// <param name="request">The WebAuthn registration request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>WebAuthn registration options if successful</returns>
    /// <response code="200">Returns registration options when initiation is successful</response>
    /// <response code="400">Returns error message when initiation fails</response>
    [HttpPost("webauthn/register/init")]
    [ProducesResponseType(typeof(Result<WebAuthnRegistrationOptionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<WebAuthnRegistrationOptionsResponse>>> InitiateWebAuthnRegistration(
        [FromBody] WebAuthnRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.InitiateWebAuthnRegistrationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Completes WebAuthn registration for a user
    /// </summary>
    /// <param name="request">The WebAuthn registration completion request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A success message if registration is completed successfully</returns>
    /// <response code="200">Returns success message when registration is completed</response>
    /// <response code="400">Returns error message when registration completion fails</response>
    [HttpPost("webauthn/register/complete")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> CompleteWebAuthnRegistration(
        [FromBody] CompleteWebAuthnRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.CompleteWebAuthnRegistrationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Initiates WebAuthn authentication for a user
    /// </summary>
    /// <param name="request">The WebAuthn authentication request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>WebAuthn authentication options if successful</returns>
    /// <response code="200">Returns authentication options when initiation is successful</response>
    /// <response code="400">Returns error message when initiation fails</response>
    [HttpPost("webauthn/authenticate/init")]
    [ProducesResponseType(typeof(Result<WebAuthnAuthenticationOptionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<WebAuthnAuthenticationOptionsResponse>>> InitiateWebAuthnAuthentication(
        [FromBody] WebAuthnAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.InitiateWebAuthnAuthenticationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Completes WebAuthn authentication for a user
    /// </summary>
    /// <param name="request">The WebAuthn authentication completion request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when authentication is successful</response>
    /// <response code="400">Returns error message when authentication fails</response>
    [HttpPost("webauthn/authenticate/complete")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> CompleteWebAuthnAuthentication(
        [FromBody] CompleteWebAuthnAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _passwordlessAuthService.CompleteWebAuthnAuthenticationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}