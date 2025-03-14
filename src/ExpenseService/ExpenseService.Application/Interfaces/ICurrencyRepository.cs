using ExpenseService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(int id);
    Task<Currency?> GetByCodeAsync(string code);
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<IEnumerable<Currency>> GetActiveAsync();
    Task<Currency?> GetDefaultAsync();
    Task AddAsync(Currency currency);
    void Update(Currency currency);
    void Delete(Currency currency);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCodeAsync(string code);
    Task SaveChangesAsync();
}