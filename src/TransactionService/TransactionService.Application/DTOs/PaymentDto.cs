using System;
using System.Collections.Generic;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.DTOs;

public class PaymentProviderDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public bool IsEnabled { get; set; }
    public List<string> SupportedPaymentMethods { get; set; }
    public decimal TransactionFeePercentage { get; set; }
    public decimal FixedTransactionFee { get; set; }
}

public class PaymentMethodDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PaymentProviderId { get; set; }
    public string MethodType { get; set; }
    public string AccountNumber { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePaymentMethodRequest
{
    public int UserId { get; set; }
    public string PaymentProviderId { get; set; }
    public string MethodType { get; set; }
    public string AccountNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Token { get; set; }
    public bool SetAsDefault { get; set; }
}

public class ProcessPaymentRequest
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public int? PaymentMethodId { get; set; }
    public string PaymentProviderId { get; set; }
    public string ExternalReferenceId { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public class PaymentResultDto
{
    public Guid TransactionId { get; set; }
    public string PaymentProviderId { get; set; }
    public string ProviderTransactionId { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ReceiptUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}