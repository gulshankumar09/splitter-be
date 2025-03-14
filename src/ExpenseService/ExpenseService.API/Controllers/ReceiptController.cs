using ExpenseService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExpenseService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReceiptController : ControllerBase
{
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<ReceiptController> _logger;

    public ReceiptController(
        IImageUploadService imageUploadService,
        ILogger<ReceiptController> logger)
    {
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    [HttpPost("{expenseId}")]
    public async Task<ActionResult<Result<string>>> UploadReceipt(int expenseId)
    {
        try
        {
            if (Request.Form.Files.Count == 0)
            {
                return BadRequest(Result<string>.Failure("No file was uploaded"));
            }

            var file = Request.Form.Files[0];
            if (file.Length == 0)
            {
                return BadRequest(Result<string>.Failure("File is empty"));
            }

            using (var stream = file.OpenReadStream())
            {
                var result = await _imageUploadService.UploadReceiptAsync(expenseId, stream, file.FileName);
                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading receipt for expense {ExpenseId}", expenseId);
            return StatusCode(500, Result<string>.Failure("An error occurred while uploading the receipt"));
        }
    }

    [HttpGet("{expenseId}/{fileName}")]
    public async Task<ActionResult<Result<byte[]>>> GetReceipt(int expenseId, string fileName)
    {
        try
        {
            var result = await _imageUploadService.GetReceiptAsync(expenseId, fileName);
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receipt {FileName} for expense {ExpenseId}", fileName, expenseId);
            return StatusCode(500, Result<byte[]>.Failure("An error occurred while retrieving the receipt"));
        }
    }

    [HttpDelete("{expenseId}/{fileName}")]
    public async Task<ActionResult<Result<bool>>> DeleteReceipt(int expenseId, string fileName)
    {
        try
        {
            var result = await _imageUploadService.DeleteReceiptAsync(expenseId, fileName);
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt {FileName} for expense {ExpenseId}", fileName, expenseId);
            return StatusCode(500, Result<bool>.Failure("An error occurred while deleting the receipt"));
        }
    }
}