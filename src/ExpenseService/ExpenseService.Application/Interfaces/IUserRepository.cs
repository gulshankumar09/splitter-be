using SharedLibrary.Models;

namespace ExpenseService.Application.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    void Update(User user);
    Task SaveChangesAsync();
}