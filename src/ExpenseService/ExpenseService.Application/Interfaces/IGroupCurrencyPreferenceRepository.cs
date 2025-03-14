using ExpenseService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IGroupCurrencyPreferenceRepository
{
    Task<GroupCurrencyPreference?> GetByIdAsync(int id);
    Task<GroupCurrencyPreference?> GetByGroupIdAsync(int groupId);
    Task<IEnumerable<GroupCurrencyPreference>> GetAllAsync();
    Task AddAsync(GroupCurrencyPreference preference);
    void Update(GroupCurrencyPreference preference);
    void Delete(GroupCurrencyPreference preference);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByGroupIdAsync(int groupId);
    Task SaveChangesAsync();
}