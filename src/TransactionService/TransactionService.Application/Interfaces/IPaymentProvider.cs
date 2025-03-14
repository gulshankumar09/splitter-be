using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Interfaces;

/// <summary>
/// Interface for payment providers like PayPal, Venmo, etc.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Gets the ID of the payment provider
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the payment provider
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the URL to the provider's logo
    /// </summary>
    string LogoUrl { get; }

    /// <summary>
    /// Gets the list of supported payment methods (credit card, bank account, etc.)
    /// </summary>
    IEnumerable<string> SupportedPaymentMethods { get; }

    /// <summary>
    /// Gets whether the provider is enabled
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the transaction fee percentage (0-100)
    /// </summary>
    decimal TransactionFeePercentage { get; }

    /// <summary>
    /// Gets the fixed transaction fee amount
    /// </summary>
    decimal FixedTransactionFee { get; }

    /// <summary>
    /// Creates a payment method token from the raw data
    /// </summary>
    Task<string> CreatePaymentMethodTokenAsync(CreatePaymentMethodRequest request);

    /// <summary>
    /// Process a payment through the payment provider
    /// </summary>
    Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentRequest request);

    /// <summary>
    /// Get the status of a payment from the payment provider
    /// </summary>
    Task<PaymentResultDto> GetPaymentStatusAsync(string providerTransactionId);

    /// <summary>
    /// Cancel a payment if possible
    /// </summary>
    Task<bool> CancelPaymentAsync(string providerTransactionId);

    /// <summary>
    /// Generate a receipt URL for a completed payment
    /// </summary>
    Task<string> GenerateReceiptUrlAsync(string providerTransactionId);
}