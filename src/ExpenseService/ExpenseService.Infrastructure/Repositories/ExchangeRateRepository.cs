using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ExpenseDbContext _context;

    public ExchangeRateRepository(ExpenseDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetByIdAsync(int id)
    {
        return await _context.ExchangeRates.FindAsync(id);
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string fromCurrency, string toCurrency)
    {
        return await _context.ExchangeRates
            .Where(er => er.FromCurrencyCode.ToLower() == fromCurrency.ToLower() &&
                         er.ToCurrencyCode.ToLower() == toCurrency.ToLower())
            .OrderByDescending(er => er.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllAsync()
    {
        return await _context.ExchangeRates.ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetByCurrencyPairAsync(string fromCurrency, string toCurrency)
    {
        return await _context.ExchangeRates
            .Where(er => er.FromCurrencyCode.ToLower() == fromCurrency.ToLower() &&
                         er.ToCurrencyCode.ToLower() == toCurrency.ToLower())
            .OrderByDescending(er => er.EffectiveDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ExchangeRates
            .Where(er => er.EffectiveDate >= startDate && er.EffectiveDate <= endDate)
            .OrderByDescending(er => er.EffectiveDate)
            .ToListAsync();
    }

    public async Task AddAsync(ExchangeRate exchangeRate)
    {
        await _context.ExchangeRates.AddAsync(exchangeRate);
    }

    public void Update(ExchangeRate exchangeRate)
    {
        _context.ExchangeRates.Update(exchangeRate);
    }

    public void Delete(ExchangeRate exchangeRate)
    {
        _context.ExchangeRates.Remove(exchangeRate);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ExchangeRates.AnyAsync(er => er.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}