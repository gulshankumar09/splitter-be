using SharedLibrary.Domain;
using System;

namespace ExpenseService.Domain.Entities;

public class BankTransaction : BaseEntity
{
    public int BankAccountId { get; private set; }
    public string ExternalId { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public string OriginalCategory { get; private set; }
    public string MerchantName { get; private set; }
    public bool IsPending { get; private set; }
    public int? LinkedExpenseId { get; private set; }
    public DateTime ImportedAt { get; private set; }

    // For EF Core
    private BankTransaction() { }

    public BankTransaction(
        int bankAccountId,
        string externalId,
        DateTime transactionDate,
        decimal amount,
        string description,
        string originalCategory,
        string merchantName,
        bool isPending)
    {
        BankAccountId = bankAccountId;
        ExternalId = externalId;
        TransactionDate = transactionDate;
        Amount = amount;
        Description = description;
        OriginalCategory = originalCategory;
        MerchantName = merchantName;
        IsPending = isPending;
        ImportedAt = DateTime.UtcNow;
    }

    public void LinkToExpense(int expenseId)
    {
        LinkedExpenseId = expenseId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePendingStatus(bool isPending, decimal? finalAmount = null)
    {
        IsPending = isPending;

        if (finalAmount.HasValue)
        {
            Amount = finalAmount.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}