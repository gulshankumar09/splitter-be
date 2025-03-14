using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services;

public class BankingIntegrationService : IBankingIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ILogger<BankingIntegrationService> _logger;
    private readonly BankingApiOptions _options;

    public BankingIntegrationService(
        IHttpClientFactory httpClientFactory,
        IExpenseRepository expenseRepository,
        IBankAccountRepository bankAccountRepository,
        IOptions<BankingApiOptions> options,
        ILogger<BankingIntegrationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _expenseRepository = expenseRepository;
        _bankAccountRepository = bankAccountRepository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<BankAccountDto>> ConnectBankAccountAsync(ConnectBankAccountRequest request)
    {
        try
        {
            // Validate request
            if (request == null || string.IsNullOrEmpty(request.BankingProviderId))
            {
                return Result<BankAccountDto>.Failure("Invalid banking provider information.");
            }

            // Get access token using the public token (specific to banking API provider)
            var tokenExchangeResult = await ExchangePublicTokenAsync(request.BankingProviderId, request.PublicToken);
            if (!tokenExchangeResult.IsSuccess)
            {
                return Result<BankAccountDto>.Failure(tokenExchangeResult.Error);
            }

            // Create new bank account record
            var bankAccount = new BankAccount(
                request.UserId,
                request.BankingProviderId,
                tokenExchangeResult.AccessToken,
                request.AccountId,
                tokenExchangeResult.BankName,
                request.AccountType);

            // Save to database
            await _bankAccountRepository.AddAsync(bankAccount);
            await _bankAccountRepository.SaveChangesAsync();

            return Result<BankAccountDto>.Success(MapToDto(bankAccount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting bank account for user {UserId}", request.UserId);
            return Result<BankAccountDto>.Failure($"Error connecting bank account: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BankAccountDto>>> GetConnectedAccountsAsync(int userId)
    {
        try
        {
            var accounts = await _bankAccountRepository.GetByUserIdAsync(userId);
            return Result<IEnumerable<BankAccountDto>>.Success(accounts.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connected bank accounts for user {UserId}", userId);
            return Result<IEnumerable<BankAccountDto>>.Failure($"Error retrieving connected bank accounts: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DisconnectBankAccountAsync(int accountId)
    {
        try
        {
            var account = await _bankAccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return Result<bool>.Failure($"Bank account with ID {accountId} not found");
            }

            account.Deactivate();
            _bankAccountRepository.Update(account);
            await _bankAccountRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting bank account {AccountId}", accountId);
            return Result<bool>.Failure($"Error disconnecting bank account: {ex.Message}");
        }
    }

    public async Task<Result<ImportTransactionsResultDto>> ImportTransactionsAsync(ImportTransactionsRequest request)
    {
        try
        {
            var account = await _bankAccountRepository.GetByIdAsync(request.BankAccountId);
            if (account == null)
            {
                return Result<ImportTransactionsResultDto>.Failure($"Bank account with ID {request.BankAccountId} not found");
            }

            if (!account.IsActive)
            {
                return Result<ImportTransactionsResultDto>.Failure($"Bank account is not active");
            }

            // Get transactions from banking API
            var transactions = await GetTransactionsFromApiAsync(
                account.BankingProviderId,
                account.AccessToken,
                account.ExternalAccountId,
                request.StartDate,
                request.EndDate);

            var result = new ImportTransactionsResultDto
            {
                BankAccountId = account.Id,
                BankName = account.BankName,
                AccountName = account.AccountName,
                TotalTransactionsFound = transactions.Count,
                ImportedTransactions = new List<BankTransactionDto>()
            };

            // Skip already imported transactions
            var existingTransactionIds = new HashSet<string>();
            if (request.SkipDuplicates)
            {
                var existingTransactions = await _bankAccountRepository.GetImportedTransactionIdsAsync(account.Id);
                existingTransactionIds = new HashSet<string>(existingTransactions);
            }

            // Process each transaction
            foreach (var transaction in transactions)
            {
                // Skip if duplicate
                if (request.SkipDuplicates && existingTransactionIds.Contains(transaction.ExternalId))
                {
                    result.DuplicatesSkipped++;
                    continue;
                }

                // Map category if auto-categorize is enabled
                if (request.AutoCategorize)
                {
                    transaction.MappedCategory = MapBankCategoryToExpenseCategory(transaction.Category, transaction.Description);
                }

                // Add to imported transactions
                result.ImportedTransactions.Add(transaction);
                result.NewTransactionsImported++;

                // Create expense if requested
                if (request.CreateExpenses)
                {
                    var expense = await CreateExpenseFromTransactionAsync(transaction, account.UserId);
                    if (expense != null)
                    {
                        result.CreatedExpenses.Add(expense);
                        result.ExpensesCreated++;
                    }
                }

                // Mark transaction as imported
                await _bankAccountRepository.AddImportedTransactionAsync(account.Id, transaction.ExternalId);
            }

            // Update last synced time
            account.UpdateLastSyncedTime();
            _bankAccountRepository.Update(account);
            await _bankAccountRepository.SaveChangesAsync();

            return Result<ImportTransactionsResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing transactions for bank account {AccountId}", request.BankAccountId);
            return Result<ImportTransactionsResultDto>.Failure($"Error importing transactions: {ex.Message}");
        }
    }

    public async Task<Result<List<ImportTransactionsResultDto>>> ImportAllTransactionsAsync()
    {
        try
        {
            var results = new List<ImportTransactionsResultDto>();
            var activeAccounts = await _bankAccountRepository.GetActiveAccountsAsync();

            foreach (var account in activeAccounts)
            {
                var request = new ImportTransactionsRequest
                {
                    BankAccountId = account.Id,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow,
                    SkipDuplicates = true,
                    AutoCategorize = true,
                    CreateExpenses = true
                };

                var result = await ImportTransactionsAsync(request);
                if (result.IsSuccess)
                {
                    results.Add(result.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to import transactions for account {AccountId}: {Error}",
                        account.Id, result.Error);
                }
            }

            return Result<List<ImportTransactionsResultDto>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing transactions for all accounts");
            return Result<List<ImportTransactionsResultDto>>.Failure($"Error importing transactions: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SyncTransactionStatusAsync(int bankAccountId)
    {
        try
        {
            var account = await _bankAccountRepository.GetByIdAsync(bankAccountId);
            if (account == null)
            {
                return Result<bool>.Failure($"Bank account with ID {bankAccountId} not found");
            }

            if (!account.IsActive)
            {
                return Result<bool>.Failure($"Bank account is not active");
            }

            // Get pending transactions from our database
            var pendingTransactions = await _bankAccountRepository.GetPendingTransactionsAsync(account.Id);
            if (!pendingTransactions.Any())
            {
                return Result<bool>.Success(true); // No pending transactions to check
            }

            // Get updated status from banking API
            var updatedStatuses = await GetTransactionStatusesFromApiAsync(
                account.BankingProviderId,
                account.AccessToken,
                pendingTransactions.Select(t => t.ExternalId).ToList());

            // Update transaction statuses
            foreach (var statusUpdate in updatedStatuses)
            {
                var pendingTx = pendingTransactions.FirstOrDefault(t => t.ExternalId == statusUpdate.TransactionId);
                if (pendingTx != null && !statusUpdate.IsPending)
                {
                    // Update the expense if linked
                    if (pendingTx.LinkedExpenseId.HasValue)
                    {
                        var expense = await _expenseRepository.GetByIdAsync(pendingTx.LinkedExpenseId.Value);
                        if (expense != null)
                        {
                            // Update expense details if needed
                            if (expense.Amount != Math.Abs(statusUpdate.FinalAmount))
                            {
                                expense.UpdateAmount(Math.Abs(statusUpdate.FinalAmount));
                                _expenseRepository.Update(expense);
                            }
                        }
                    }

                    // Mark transaction as settled
                    await _bankAccountRepository.UpdateTransactionStatusAsync(
                        account.Id,
                        statusUpdate.TransactionId,
                        false,
                        statusUpdate.FinalAmount);
                }
            }

            await _bankAccountRepository.SaveChangesAsync();
            await _expenseRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing transaction status for bank account {AccountId}", bankAccountId);
            return Result<bool>.Failure($"Error syncing transaction status: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BankingProviderDto>>> GetAvailableBankingProvidersAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BankingApi");
            var providers = await client.GetFromJsonAsync<List<BankingProviderDto>>($"/providers");

            if (providers == null)
            {
                return Result<IEnumerable<BankingProviderDto>>.Failure("Failed to retrieve banking providers");
            }

            return Result<IEnumerable<BankingProviderDto>>.Success(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available banking providers");
            return Result<IEnumerable<BankingProviderDto>>.Failure($"Error retrieving banking providers: {ex.Message}");
        }
    }

    #region Helper Methods

    private async Task<(bool IsSuccess, string AccessToken, string BankName, string Error)> ExchangePublicTokenAsync(
        string providerId, string publicToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BankingApi");
            var response = await client.PostAsJsonAsync("/exchange_token", new
            {
                provider_id = providerId,
                public_token = publicToken,
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, null, $"Failed to exchange token: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (result == null || !result.ContainsKey("access_token") || !result.ContainsKey("institution_name"))
            {
                return (false, null, null, "Invalid response from banking API");
            }

            return (true, result["access_token"], result["institution_name"], null);
        }
        catch (Exception ex)
        {
            return (false, null, null, $"Error exchanging public token: {ex.Message}");
        }
    }

    private async Task<List<BankTransactionDto>> GetTransactionsFromApiAsync(
        string providerId, string accessToken, string accountId, DateTime? startDate, DateTime? endDate)
    {
        var client = _httpClientFactory.CreateClient("BankingApi");

        // Default to last 30 days if no dates specified
        var start = startDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var end = endDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await client.PostAsJsonAsync("/transactions/get", new
        {
            provider_id = providerId,
            access_token = accessToken,
            account_id = accountId,
            start_date = start,
            end_date = end
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get transactions: {response.StatusCode}");
        }

        // This structure would depend on the actual API you're using
        var apiResponse = await response.Content.ReadFromJsonAsync<BankApiTransactionsResponse>();
        if (apiResponse?.Transactions == null)
        {
            return new List<BankTransactionDto>();
        }

        // Map to our DTO
        return apiResponse.Transactions.Select(t => new BankTransactionDto
        {
            ExternalId = t.TransactionId,
            TransactionDate = t.Date,
            Amount = Math.Abs(t.Amount), // Store as positive amount
            Description = t.Description,
            Category = t.Category,
            MerchantName = t.MerchantName,
            IsPending = t.Pending
        }).ToList();
    }

    private async Task<List<TransactionStatusUpdate>> GetTransactionStatusesFromApiAsync(
        string providerId, string accessToken, List<string> transactionIds)
    {
        var client = _httpClientFactory.CreateClient("BankingApi");

        var response = await client.PostAsJsonAsync("/transactions/status", new
        {
            provider_id = providerId,
            access_token = accessToken,
            transaction_ids = transactionIds
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get transaction statuses: {response.StatusCode}");
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<BankApiTransactionStatusResponse>();
        if (apiResponse?.Transactions == null)
        {
            return new List<TransactionStatusUpdate>();
        }

        return apiResponse.Transactions.Select(t => new TransactionStatusUpdate
        {
            TransactionId = t.TransactionId,
            IsPending = t.Pending,
            FinalAmount = t.Amount
        }).ToList();
    }

    private ExpenseCategory MapBankCategoryToExpenseCategory(string bankCategory, string description)
    {
        // Simple mapping logic - in a real application this would be more sophisticated
        // and might use machine learning or user preferences

        var lowerCategory = bankCategory?.ToLowerInvariant() ?? "";
        var lowerDescription = description?.ToLowerInvariant() ?? "";

        if (lowerCategory.Contains("food") || lowerCategory.Contains("restaurant") ||
            lowerDescription.Contains("restaurant") || lowerDescription.Contains("cafe"))
        {
            return ExpenseCategory.Food;
        }

        if (lowerCategory.Contains("transport") || lowerCategory.Contains("uber") ||
            lowerCategory.Contains("taxi") || lowerDescription.Contains("lyft"))
        {
            return ExpenseCategory.Transportation;
        }

        if (lowerCategory.Contains("entertainment") || lowerCategory.Contains("movie") ||
            lowerDescription.Contains("netflix") || lowerDescription.Contains("spotify"))
        {
            return ExpenseCategory.Entertainment;
        }

        if (lowerCategory.Contains("shop") || lowerCategory.Contains("merchandise") ||
            lowerDescription.Contains("amazon") || lowerDescription.Contains("walmart"))
        {
            return ExpenseCategory.Shopping;
        }

        if (lowerCategory.Contains("util") || lowerCategory.Contains("bill") ||
            lowerDescription.Contains("electric") || lowerDescription.Contains("water"))
        {
            return ExpenseCategory.Utilities;
        }

        if (lowerCategory.Contains("health") || lowerCategory.Contains("medical") ||
            lowerDescription.Contains("pharmacy") || lowerDescription.Contains("doctor"))
        {
            return ExpenseCategory.Healthcare;
        }

        if (lowerCategory.Contains("travel") || lowerCategory.Contains("hotel") ||
            lowerDescription.Contains("airbnb") || lowerDescription.Contains("airline"))
        {
            return ExpenseCategory.Travel;
        }

        return ExpenseCategory.Other;
    }

    private async Task<ExpenseDto> CreateExpenseFromTransactionAsync(BankTransactionDto transaction, int userId)
    {
        try
        {
            // Skip income transactions (positive amounts are typically deposits in banking APIs)
            if (transaction.Amount <= 0)
            {
                return null;
            }

            var category = transaction.MappedCategory ?? ExpenseCategory.Other;

            var expense = new Expense(
                transaction.Description ?? transaction.MerchantName ?? "Bank Transaction",
                transaction.Amount,
                userId,
                category,
                transaction.TransactionDate
            );

            // Add a tag to indicate this was auto-imported
            expense.AddTag("auto-imported");
            if (!string.IsNullOrEmpty(transaction.MerchantName))
            {
                expense.AddTag(transaction.MerchantName.ToLowerInvariant().Replace(" ", "-"));
            }

            // Add a note with the external transaction ID for reference
            expense.SetNotes($"Auto-imported from bank transaction {transaction.ExternalId}. Original category: {transaction.Category}");

            await _expenseRepository.AddAsync(expense);
            await _expenseRepository.SaveChangesAsync();

            // Link the expense to the transaction in our database
            await _bankAccountRepository.LinkTransactionToExpenseAsync(transaction.ExternalId, expense.Id);

            return MapExpenseToDto(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense from transaction {TransactionId}", transaction.ExternalId);
            return null;
        }
    }

    private BankAccountDto MapToDto(BankAccount account)
    {
        return new BankAccountDto
        {
            Id = account.Id,
            UserId = account.UserId,
            BankName = account.BankName,
            AccountName = account.AccountName,
            AccountNumber = account.MaskedAccountNumber,
            AccountType = account.AccountType,
            BankingProviderId = account.BankingProviderId,
            IsActive = account.IsActive,
            LastSyncedAt = account.LastSyncedAt,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }

    private ExpenseDto MapExpenseToDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            CurrencyCode = expense.CurrencyCode,
            PaidByUserId = expense.PaidByUserId,
            Category = expense.Category,
            Notes = expense.Notes,
            Tags = expense.Tags.ToList(),
            ExpenseDate = expense.ExpenseDate,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }

    #endregion
}

// Helper classes for API responses
public class BankApiTransactionsResponse
{
    public List<BankApiTransaction> Transactions { get; set; }
}

public class BankApiTransaction
{
    public string TransactionId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string MerchantName { get; set; }
    public bool Pending { get; set; }
}

public class BankApiTransactionStatusResponse
{
    public List<BankApiTransactionStatus> Transactions { get; set; }
}

public class BankApiTransactionStatus
{
    public string TransactionId { get; set; }
    public bool Pending { get; set; }
    public decimal Amount { get; set; }
}

public class TransactionStatusUpdate
{
    public string TransactionId { get; set; }
    public bool IsPending { get; set; }
    public decimal FinalAmount { get; set; }
}

public class BankingApiOptions
{
    public string BaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}