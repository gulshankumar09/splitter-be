using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories
{
    public class UserCurrencyPreferenceRepository : IUserCurrencyPreferenceRepository
    {
        private readonly ExpenseDbContext _context;

        public UserCurrencyPreferenceRepository(ExpenseDbContext context)
        {
            _context = context;
        }

        public async Task<UserCurrencyPreference?> GetByIdAsync(int id)
        {
            return await _context.UserCurrencyPreferences.FindAsync(id);
        }

        public async Task<UserCurrencyPreference?> GetByUserIdAsync(int userId)
        {
            return await _context.UserCurrencyPreferences
                .FirstOrDefaultAsync(ucp => ucp.UserId == userId);
        }

        public async Task<IEnumerable<UserCurrencyPreference>> GetAllAsync()
        {
            return await _context.UserCurrencyPreferences.ToListAsync();
        }

        public async Task AddAsync(UserCurrencyPreference preference)
        {
            await _context.UserCurrencyPreferences.AddAsync(preference);
        }

        public void Update(UserCurrencyPreference preference)
        {
            _context.UserCurrencyPreferences.Update(preference);
        }

        public void Delete(UserCurrencyPreference preference)
        {
            _context.UserCurrencyPreferences.Remove(preference);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UserCurrencyPreferences.AnyAsync(ucp => ucp.Id == id);
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.UserCurrencyPreferences.AnyAsync(ucp => ucp.UserId == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}