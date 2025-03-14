using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services
{
    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly IExchangeRateRepository _exchangeRateRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyConversionService> _logger;

        public CurrencyConversionService(
            IExchangeRateRepository exchangeRateRepository,
            ICurrencyRepository currencyRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<CurrencyConversionService> logger)
        {
            _exchangeRateRepository = exchangeRateRepository;
            _currencyRepository = currencyRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<CurrencyConversionDto> ConvertCurrencyAsync(string fromCurrencyCode, string toCurrencyCode, decimal amount)
        {
            if (string.Equals(fromCurrencyCode, toCurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return new CurrencyConversionDto
                {
                    FromCurrencyCode = fromCurrencyCode,
                    ToCurrencyCode = toCurrencyCode,
                    Amount = amount,
                    ConvertedAmount = amount,
                    ExchangeRate = 1.0m,
                    ConversionDate = DateTime.UtcNow
                };
            }

            var rate = await GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode);
            var convertedAmount = amount * rate;

            return new CurrencyConversionDto
            {
                FromCurrencyCode = fromCurrencyCode,
                ToCurrencyCode = toCurrencyCode,
                Amount = amount,
                ConvertedAmount = convertedAmount,
                ExchangeRate = rate,
                ConversionDate = DateTime.UtcNow
            };
        }

        public async Task<CurrencyConversionDto> ConvertCurrencyAtDateAsync(string fromCurrencyCode, string toCurrencyCode, decimal amount, DateTime date)
        {
            if (string.Equals(fromCurrencyCode, toCurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return new CurrencyConversionDto
                {
                    FromCurrencyCode = fromCurrencyCode,
                    ToCurrencyCode = toCurrencyCode,
                    Amount = amount,
                    ConvertedAmount = amount,
                    ExchangeRate = 1.0m,
                    ConversionDate = date
                };
            }

            var rate = await GetExchangeRateAtDateAsync(fromCurrencyCode, toCurrencyCode, date);
            var convertedAmount = amount * rate;

            return new CurrencyConversionDto
            {
                FromCurrencyCode = fromCurrencyCode,
                ToCurrencyCode = toCurrencyCode,
                Amount = amount,
                ConvertedAmount = convertedAmount,
                ExchangeRate = rate,
                ConversionDate = date
            };
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode)
        {
            if (string.Equals(fromCurrencyCode, toCurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return 1.0m;
            }

            var exchangeRate = await _exchangeRateRepository.GetLatestRateAsync(fromCurrencyCode, toCurrencyCode);

            if (exchangeRate != null)
            {
                return exchangeRate.Rate;
            }

            // Try the inverse rate
            exchangeRate = await _exchangeRateRepository.GetLatestRateAsync(toCurrencyCode, fromCurrencyCode);

            if (exchangeRate != null)
            {
                return 1 / exchangeRate.Rate;
            }

            // If we don't have a direct rate, try to find a rate through USD
            if (fromCurrencyCode != "USD" && toCurrencyCode != "USD")
            {
                var fromToUsdRate = await GetExchangeRateAsync(fromCurrencyCode, "USD");
                var usdToToRate = await GetExchangeRateAsync("USD", toCurrencyCode);
                return fromToUsdRate * usdToToRate;
            }

            throw new InvalidOperationException($"Exchange rate not found for {fromCurrencyCode} to {toCurrencyCode}");
        }

        public async Task<decimal> GetExchangeRateAtDateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime date)
        {
            if (string.Equals(fromCurrencyCode, toCurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return 1.0m;
            }

            // Get all rates for the currency pair
            var rates = await _exchangeRateRepository.GetByCurrencyPairAsync(fromCurrencyCode, toCurrencyCode);

            // Find the closest rate to the requested date
            ExchangeRate? closestRate = null;
            TimeSpan closestDifference = TimeSpan.MaxValue;

            foreach (var rate in rates)
            {
                var difference = rate.EffectiveDate > date
                    ? rate.EffectiveDate - date
                    : date - rate.EffectiveDate;

                if (difference < closestDifference)
                {
                    closestRate = rate;
                    closestDifference = difference;
                }
            }

            if (closestRate != null)
            {
                return closestRate.Rate;
            }

            // Try the inverse rate
            rates = await _exchangeRateRepository.GetByCurrencyPairAsync(toCurrencyCode, fromCurrencyCode);

            closestRate = null;
            closestDifference = TimeSpan.MaxValue;

            foreach (var rate in rates)
            {
                var difference = rate.EffectiveDate > date
                    ? rate.EffectiveDate - date
                    : date - rate.EffectiveDate;

                if (difference < closestDifference)
                {
                    closestRate = rate;
                    closestDifference = difference;
                }
            }

            if (closestRate != null)
            {
                return 1 / closestRate.Rate;
            }

            // If we don't have a direct rate, try to find a rate through USD
            if (fromCurrencyCode != "USD" && toCurrencyCode != "USD")
            {
                var fromToUsdRate = await GetExchangeRateAtDateAsync(fromCurrencyCode, "USD", date);
                var usdToToRate = await GetExchangeRateAtDateAsync("USD", toCurrencyCode, date);
                return fromToUsdRate * usdToToRate;
            }

            throw new InvalidOperationException($"Exchange rate not found for {fromCurrencyCode} to {toCurrencyCode} at date {date}");
        }

        public async Task UpdateExchangeRatesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ExchangeRateApi");

                // Get all currencies
                var currencies = await _currencyRepository.GetActiveAsync();
                var defaultCurrency = await _currencyRepository.GetDefaultAsync();
                string baseCurrency = defaultCurrency?.Code ?? "USD";

                // For demonstration, using a simple API structure
                // In reality, you would use an actual currency exchange API
                var response = await client.GetFromJsonAsync<ExchangeRateApiResponse>($"/latest?base={baseCurrency}");

                if (response == null || response.Rates == null)
                {
                    _logger.LogError("Failed to retrieve exchange rates from API");
                    return;
                }

                var today = DateTime.UtcNow.Date;

                foreach (var currency in currencies)
                {
                    if (currency.Code == baseCurrency)
                    {
                        continue; // Skip the base currency
                    }

                    if (response.Rates.TryGetValue(currency.Code, out var rate))
                    {
                        var existingRate = await _exchangeRateRepository.GetLatestRateAsync(baseCurrency, currency.Code);

                        if (existingRate != null && existingRate.EffectiveDate.Date == today)
                        {
                            // Update existing rate using the method provided by the entity
                            existingRate.UpdateRate(rate, today, "API");
                            _exchangeRateRepository.Update(existingRate);
                        }
                        else
                        {
                            // Create new rate
                            var newRate = new ExchangeRate(
                                baseCurrency,
                                currency.Code,
                                rate,
                                today,
                                "API"
                            );
                            await _exchangeRateRepository.AddAsync(newRate);
                        }
                    }
                }

                await _exchangeRateRepository.SaveChangesAsync();
                _logger.LogInformation("Exchange rates updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates");
                throw;
            }
        }

        // Simple class to represent the response from an exchange rate API
        private class ExchangeRateApiResponse
        {
            public string? Base { get; set; }
            public DateTime Date { get; set; }
            public Dictionary<string, decimal> Rates { get; set; } = new();
        }
    }
}