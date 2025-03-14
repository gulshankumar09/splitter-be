using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories
{
    public class GroupCurrencyPreferenceRepository : IGroupCurrencyPreferenceRepository
    {
        private readonly ExpenseDbContext _context;

        public GroupCurrencyPreferenceRepository(ExpenseDbContext context)
        {
            _context = context;
        }

        public async Task<GroupCurrencyPreference?> GetByIdAsync(int id)
        {
            return await _context.GroupCurrencyPreferences.FindAsync(id);
        }

        public async Task<GroupCurrencyPreference?> GetByGroupIdAsync(int groupId)
        {
            return await _context.GroupCurrencyPreferences
                .FirstOrDefaultAsync(gcp => gcp.GroupId == groupId);
        }

        public async Task<IEnumerable<GroupCurrencyPreference>> GetAllAsync()
        {
            return await _context.GroupCurrencyPreferences.ToListAsync();
        }

        public async Task AddAsync(GroupCurrencyPreference preference)
        {
            await _context.GroupCurrencyPreferences.AddAsync(preference);
        }

        public void Update(GroupCurrencyPreference preference)
        {
            _context.GroupCurrencyPreferences.Update(preference);
        }

        public void Delete(GroupCurrencyPreference preference)
        {
            _context.GroupCurrencyPreferences.Remove(preference);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.GroupCurrencyPreferences.AnyAsync(gcp => gcp.Id == id);
        }

        public async Task<bool> ExistsByGroupIdAsync(int groupId)
        {
            return await _context.GroupCurrencyPreferences.AnyAsync(gcp => gcp.GroupId == groupId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}