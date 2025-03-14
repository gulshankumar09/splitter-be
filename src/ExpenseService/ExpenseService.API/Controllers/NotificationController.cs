using ExpenseService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Threading.Tasks;

namespace ExpenseService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("device-token")]
    public async Task<ActionResult<Result<bool>>> UpdateDeviceToken([FromBody] UpdateDeviceTokenRequest request)
    {
        try
        {
            var userId = int.Parse(User.Identity.Name); // Assuming the user ID is stored in the Name claim
            var result = await _notificationService.UpdateUserDeviceTokenAsync(userId, request.DeviceToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device token");
            return StatusCode(500, Result<bool>.Failure("An error occurred while updating the device token"));
        }
    }

    [HttpPost("test")]
    public async Task<ActionResult<Result<bool>>> SendTestNotification()
    {
        try
        {
            var userId = int.Parse(User.Identity.Name);
            var result = await _notificationService.SendExpenseReminderAsync(
                userId,
                "This is a test notification from the Expense Management System"
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            return StatusCode(500, Result<bool>.Failure("An error occurred while sending the test notification"));
        }
    }
}

public class UpdateDeviceTokenRequest
{
    public string DeviceToken { get; set; }
}