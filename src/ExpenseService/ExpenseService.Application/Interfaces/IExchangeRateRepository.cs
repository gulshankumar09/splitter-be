using ExpenseService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByIdAsync(int id);
    Task<ExchangeRate?> GetLatestRateAsync(string fromCurrency, string toCurrency);
    Task<IEnumerable<ExchangeRate>> GetAllAsync();
    Task<IEnumerable<ExchangeRate>> GetByCurrencyPairAsync(string fromCurrency, string toCurrency);
    Task<IEnumerable<ExchangeRate>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(ExchangeRate exchangeRate);
    void Update(ExchangeRate exchangeRate);
    void Delete(ExchangeRate exchangeRate);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}