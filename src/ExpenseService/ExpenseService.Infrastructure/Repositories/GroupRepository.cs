using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories;

namespace ExpenseService.Infrastructure.Repositories;

public class GroupRepository : GenericRepository<Group>, IGroupRepository
{
    private readonly ExpenseDbContext _context;

    public GroupRepository(ExpenseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .Include(g => g.Expenses)
                .ThenInclude(e => e.Splits)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        return await _context.Groups
            .Include(g => g.Members)
            .ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetByUserIdAsync(int userId)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetByTypeAsync(GroupType type)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .Where(g => g.Type == type)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Groups.AnyAsync(g => g.Id == id);
    }

    public void Update(Group group)
    {
        _context.Groups.Update(group);
    }

    public void Delete(Group group)
    {
        _context.Groups.Remove(group);
    }
}