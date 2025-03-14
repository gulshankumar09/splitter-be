using ExpenseService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class ImageUploadService : IImageUploadService
{
    private readonly ILogger<ImageUploadService> _logger;
    private readonly ImageUploadOptions _options;
    private readonly IExpenseRepository _expenseRepository;

    public ImageUploadService(
        ILogger<ImageUploadService> logger,
        IOptions<ImageUploadOptions> options,
        IExpenseRepository expenseRepository)
    {
        _logger = logger;
        _options = options.Value;
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<string>> UploadReceiptAsync(int expenseId, Stream imageStream, string fileName)
    {
        try
        {
            // Validate expense exists
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<string>.Failure($"Expense with ID {expenseId} not found");
            }

            // Validate file type
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(fileExtension))
            {
                return Result<string>.Failure($"File type {fileExtension} is not allowed");
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_options.UploadPath, uniqueFileName);

            // Ensure upload directory exists
            Directory.CreateDirectory(_options.UploadPath);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Add attachment to expense
            expense.AddAttachment(fileName, fileExtension, uniqueFileName);
            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<string>.Success(uniqueFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading receipt for expense {ExpenseId}", expenseId);
            return Result<string>.Failure($"Error uploading receipt: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> GetReceiptAsync(int expenseId, string fileName)
    {
        try
        {
            // Validate expense exists
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<byte[]>.Failure($"Expense with ID {expenseId} not found");
            }

            // Find attachment
            var attachment = expense.Attachments.FirstOrDefault(a => a.FileName == fileName);
            if (attachment == null)
            {
                return Result<byte[]>.Failure($"Receipt {fileName} not found for expense {expenseId}");
            }

            // Read file
            var filePath = Path.Combine(_options.UploadPath, attachment.FileUrl);
            if (!File.Exists(filePath))
            {
                return Result<byte[]>.Failure($"Receipt file not found at {filePath}");
            }

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            return Result<byte[]>.Success(fileBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receipt {FileName} for expense {ExpenseId}", fileName, expenseId);
            return Result<byte[]>.Failure($"Error retrieving receipt: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteReceiptAsync(int expenseId, string fileName)
    {
        try
        {
            // Validate expense exists
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return Result<bool>.Failure($"Expense with ID {expenseId} not found");
            }

            // Find attachment
            var attachment = expense.Attachments.FirstOrDefault(a => a.FileName == fileName);
            if (attachment == null)
            {
                return Result<bool>.Failure($"Receipt {fileName} not found for expense {expenseId}");
            }

            // Delete file
            var filePath = Path.Combine(_options.UploadPath, attachment.FileUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Remove attachment from expense
            expense.RemoveAttachment(attachment.Id);
            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt {FileName} for expense {ExpenseId}", fileName, expenseId);
            return Result<bool>.Failure($"Error deleting receipt: {ex.Message}");
        }
    }
}

public class ImageUploadOptions
{
    public string UploadPath { get; set; } = "uploads/receipts";
    public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
    public int MaxFileSizeInMB { get; set; } = 10;
}