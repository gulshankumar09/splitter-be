using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class ExpenseSplit : BaseEntity
{
    public int UserId { get; private set; }
    public decimal Amount { get; private set; }
    public SplitType SplitType { get; private set; }
    public decimal? Percentage { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidDate { get; private set; }

    // For EF Core
    private ExpenseSplit() { }

    public ExpenseSplit(int userId, decimal amount, SplitType splitType = SplitType.ExactAmount)
    {
        UserId = userId;
        Amount = amount;
        SplitType = splitType;
        IsPaid = false;
    }

    public ExpenseSplit(int userId, decimal percentage, SplitType splitType, decimal calculatedAmount)
    {
        UserId = userId;
        Percentage = percentage;
        SplitType = splitType;
        Amount = calculatedAmount;
        IsPaid = false;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
        PaidDate = DateTime.UtcNow;
    }

    public void MarkAsUnpaid()
    {
        IsPaid = false;
        PaidDate = null;
    }

    public void UpdateAmount(decimal newAmount)
    {
        Amount = newAmount;
    }
}