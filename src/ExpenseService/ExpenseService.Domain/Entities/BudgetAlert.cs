using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class BudgetAlert : BaseEntity
{
    public int BudgetId { get; private set; }
    public int UserId { get; private set; }
    public string Message { get; private set; }
    public decimal ThresholdPercentage { get; private set; }
    public decimal CurrentSpending { get; private set; }
    public decimal BudgetLimit { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // For EF Core
    private BudgetAlert() { }

    public BudgetAlert(
        int budgetId,
        int userId,
        string message,
        decimal thresholdPercentage,
        decimal currentSpending,
        decimal budgetLimit)
    {
        BudgetId = budgetId;
        UserId = userId;
        Message = message;
        ThresholdPercentage = thresholdPercentage;
        CurrentSpending = currentSpending;
        BudgetLimit = budgetLimit;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}