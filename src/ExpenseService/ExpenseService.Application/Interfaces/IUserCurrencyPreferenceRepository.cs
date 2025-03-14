using ExpenseService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IUserCurrencyPreferenceRepository
{
    Task<UserCurrencyPreference?> GetByIdAsync(int id);
    Task<UserCurrencyPreference?> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserCurrencyPreference>> GetAllAsync();
    Task AddAsync(UserCurrencyPreference preference);
    void Update(UserCurrencyPreference preference);
    void Delete(UserCurrencyPreference preference);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByUserIdAsync(int userId);
    Task SaveChangesAsync();
}