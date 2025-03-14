using ExpenseService.Domain.Entities;
using System;
using System.Collections.Generic;

namespace ExpenseService.Application.DTOs;

public class BankAccountDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BankName { get; set; }
    public string AccountName { get; set; }
    public string AccountNumber { get; set; } // Masked account number for display
    public string AccountType { get; set; }
    public string BankingProviderId { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BankingProviderDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public string Description { get; set; }
    public List<string> SupportedAccountTypes { get; set; }
    public bool RequiresOAuth { get; set; }
    public string AuthUrl { get; set; }
}

public class BankTransactionDto
{
    public string ExternalId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string MerchantName { get; set; }
    public bool IsPending { get; set; }
    public ExpenseCategory? MappedCategory { get; set; }
}

public class ConnectBankAccountRequest
{
    public int UserId { get; set; }
    public string BankingProviderId { get; set; }
    public string PublicToken { get; set; }
    public Dictionary<string, string> Credentials { get; set; }
    public string AccountId { get; set; }
    public string AccountType { get; set; }
}

public class ImportTransactionsRequest
{
    public int BankAccountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool SkipDuplicates { get; set; } = true;
    public bool AutoCategorize { get; set; } = true;
    public bool CreateExpenses { get; set; } = true;
}

public class ImportTransactionsResultDto
{
    public int BankAccountId { get; set; }
    public string BankName { get; set; }
    public string AccountName { get; set; }
    public int TotalTransactionsFound { get; set; }
    public int NewTransactionsImported { get; set; }
    public int DuplicatesSkipped { get; set; }
    public int ExpensesCreated { get; set; }
    public List<BankTransactionDto> ImportedTransactions { get; set; } = new List<BankTransactionDto>();
    public List<ExpenseDto> CreatedExpenses { get; set; } = new List<ExpenseDto>();
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}