using SharedLibrary.Domain;
using System;

namespace ExpenseService.Domain.Entities;

public class BankAccount : BaseEntity
{
    public int UserId { get; private set; }
    public string BankingProviderId { get; private set; }
    public string AccessToken { get; private set; }
    public string ExternalAccountId { get; private set; }
    public string BankName { get; private set; }
    public string AccountName { get; private set; }
    public string AccountType { get; private set; }
    public string MaskedAccountNumber { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime LastSyncedAt { get; private set; }

    // For EF Core
    private BankAccount() { }

    public BankAccount(
        int userId,
        string bankingProviderId,
        string accessToken,
        string externalAccountId,
        string bankName,
        string accountType)
    {
        UserId = userId;
        BankingProviderId = bankingProviderId;
        AccessToken = accessToken;
        ExternalAccountId = externalAccountId;
        BankName = bankName;
        AccountType = accountType;

        // Generate a masked account number for display (last 4 digits)
        if (externalAccountId.Length > 4)
        {
            MaskedAccountNumber = $"••••{externalAccountId.Substring(externalAccountId.Length - 4)}";
        }
        else
        {
            MaskedAccountNumber = externalAccountId;
        }

        // Set default values
        AccountName = $"{bankName} {accountType}";
        IsActive = true;
        LastSyncedAt = DateTime.UtcNow;
    }

    public void UpdateAccessToken(string accessToken)
    {
        AccessToken = accessToken;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAccountInfo(string bankName, string accountName, string accountType)
    {
        BankName = bankName;
        AccountName = accountName;
        AccountType = accountType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastSyncedTime()
    {
        LastSyncedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}