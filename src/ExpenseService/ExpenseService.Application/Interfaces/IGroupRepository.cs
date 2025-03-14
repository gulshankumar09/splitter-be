using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(int id);
    Task<IEnumerable<Group>> GetAllAsync();
    Task<IEnumerable<Group>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Group>> GetByTypeAsync(GroupType type);
    Task AddAsync(Group group);
    void Update(Group group);
    void Delete(Group group);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}