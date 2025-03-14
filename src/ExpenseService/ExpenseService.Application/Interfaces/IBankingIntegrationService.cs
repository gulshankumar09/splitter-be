using ExpenseService.Application.DTOs;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

/// <summary>
/// Service for integrating with banking APIs to import transactions
/// </summary>
public interface IBankingIntegrationService
{
    /// <summary>
    /// Connect a user's bank account for transaction importing
    /// </summary>
    Task<Result<BankAccountDto>> ConnectBankAccountAsync(ConnectBankAccountRequest request);

    /// <summary>
    /// Get a list of connected bank accounts for a user
    /// </summary>
    Task<Result<IEnumerable<BankAccountDto>>> GetConnectedAccountsAsync(int userId);

    /// <summary>
    /// Disconnect a bank account
    /// </summary>
    Task<Result<bool>> DisconnectBankAccountAsync(int accountId);

    /// <summary>
    /// Import transactions from a connected bank account for a specific date range
    /// </summary>
    Task<Result<ImportTransactionsResultDto>> ImportTransactionsAsync(ImportTransactionsRequest request);

    /// <summary>
    /// Import all transactions from all connected bank accounts (typically for a background job)
    /// </summary>
    Task<Result<List<ImportTransactionsResultDto>>> ImportAllTransactionsAsync();

    /// <summary>
    /// Synchronize transaction status with banking API
    /// </summary>
    Task<Result<bool>> SyncTransactionStatusAsync(int bankAccountId);

    /// <summary>
    /// Get available banking providers that the system can integrate with
    /// </summary>
    Task<Result<IEnumerable<BankingProviderDto>>> GetAvailableBankingProvidersAsync();
}