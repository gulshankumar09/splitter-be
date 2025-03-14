using SharedLibrary.Domain;
using ExpenseService.Domain.Services;

namespace ExpenseService.Domain.Entities;

public class SettlementRecord : BaseEntity
{
    public int FromUserId { get; private set; }
    public int ToUserId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    // For EF Core
    private SettlementRecord() { }

    public SettlementRecord(int fromUserId, int toUserId, decimal amount, string? notes = null)
        : base()
    {
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        Status = TransactionStatus.Pending;
        Notes = notes;
    }

    public void MarkAsCompleted()
    {
        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsCancelled()
    {
        Status = TransactionStatus.Cancelled;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }
}