using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaidByUserId { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ExpenseSplitDto> Splits { get; set; } = new();
    public List<ExpenseAttachmentDto> Attachments { get; set; } = new();
    public DateTime ExpenseDate { get; set; }
    public SplitType DefaultSplitType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ExpenseSplitDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public SplitType SplitType { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
}

public class ExpenseAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}

public class CreateExpenseRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaidByUserId { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime ExpenseDate { get; set; }
    public SplitDetails? SplitDetails { get; set; }
}

public class UpdateExpenseRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime ExpenseDate { get; set; }
    public SplitDetails? SplitDetails { get; set; }
}

public class SplitDetails
{
    public SplitType SplitType { get; set; }

    // For equal splits
    public List<int>? UserIds { get; set; }

    // For percentage splits
    public Dictionary<int, decimal>? UserPercentages { get; set; }

    // For exact amount splits
    public Dictionary<int, decimal>? UserAmounts { get; set; }

    // For uneven splits (custom)
    public List<CustomSplitDto>? CustomSplits { get; set; }
}

public class CustomSplitDto
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public SplitType SplitType { get; set; }
    public decimal? Percentage { get; set; }
}

public class AddAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}

public class MarkSplitPaidRequest
{
    public int UserId { get; set; }
}