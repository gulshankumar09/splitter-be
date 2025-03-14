using ExpenseService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface ICurrencyConversionService
{
    /// <summary>
    /// Converts an amount from one currency to another using the latest exchange rate
    /// </summary>
    Task<CurrencyConversionDto> ConvertCurrencyAsync(string fromCurrencyCode, string toCurrencyCode, decimal amount);

    /// <summary>
    /// Converts an amount from one currency to another using the exchange rate at a specific date
    /// </summary>
    Task<CurrencyConversionDto> ConvertCurrencyAtDateAsync(string fromCurrencyCode, string toCurrencyCode, decimal amount, DateTime date);

    /// <summary>
    /// Gets the latest exchange rate between two currencies
    /// </summary>
    Task<decimal> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode);

    /// <summary>
    /// Gets the exchange rate between two currencies at a specific date
    /// </summary>
    Task<decimal> GetExchangeRateAtDateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime date);

    /// <summary>
    /// Updates all exchange rates from external API
    /// </summary>
    Task UpdateExchangeRatesAsync();
}