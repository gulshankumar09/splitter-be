using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Interfaces;

public interface IPaymentService
{
    // Provider management
    Task<IEnumerable<PaymentProviderDto>> GetAvailablePaymentProvidersAsync();

    // Payment method management
    Task<IEnumerable<PaymentMethodDto>> GetUserPaymentMethodsAsync(int userId);
    Task<PaymentMethodDto> AddPaymentMethodAsync(CreatePaymentMethodRequest request);
    Task<bool> RemovePaymentMethodAsync(int paymentMethodId);
    Task<bool> SetDefaultPaymentMethodAsync(int userId, int paymentMethodId);

    // Payment processing
    Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<PaymentResultDto> GetPaymentStatusAsync(Guid transactionId);
    Task<bool> CancelPaymentAsync(Guid transactionId);

    // Receipt management
    Task<string> GenerateReceiptAsync(Guid transactionId);
}