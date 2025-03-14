using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ExpenseDbContext _context;

    public CurrencyRepository(ExpenseDbContext context)
    {
        _context = context;
    }

    public async Task<Currency?> GetByIdAsync(int id)
    {
        return await _context.Currencies.FindAsync(id);
    }

    public async Task<Currency?> GetByCodeAsync(string code)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _context.Currencies.ToListAsync();
    }

    public async Task<IEnumerable<Currency>> GetActiveAsync()
    {
        return await _context.Currencies
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    public async Task<Currency?> GetDefaultAsync()
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.IsDefault);
    }

    public async Task AddAsync(Currency currency)
    {
        await _context.Currencies.AddAsync(currency);
    }

    public void Update(Currency currency)
    {
        _context.Currencies.Update(currency);
    }

    public void Delete(Currency currency)
    {
        _context.Currencies.Remove(currency);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Currencies.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _context.Currencies.AnyAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}