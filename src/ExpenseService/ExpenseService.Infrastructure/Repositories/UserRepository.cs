using ExpenseService.Application.Interfaces;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ExpenseDbContext _context;
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ILogger<UserRepository> _logger;
    private readonly Dictionary<int, User> _userCache = new Dictionary<int, User>();

    public UserRepository(
        ExpenseDbContext context,
        IAuthServiceClient authServiceClient,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _authServiceClient = authServiceClient;
        _logger = logger;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        // First check the cache
        if (_userCache.TryGetValue(id, out var cachedUser))
        {
            return cachedUser;
        }

        // Then check the local database
        var localUser = await _context.Users.FindAsync(id);
        if (localUser != null)
        {
            _userCache[id] = localUser;
            return localUser;
        }

        // Finally, try to get from the AuthService
        try
        {
            var result = await _authServiceClient.GetUserByIdAsync(id.ToString());
            if (result.IsSuccess && result.Data != null)
            {
                // Save to local database for future use
                _context.Users.Add(result.Data);
                await _context.SaveChangesAsync();

                _userCache[id] = result.Data;
                return result.Data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId} from AuthService", id);
        }

        return null;
    }

    public void Update(User user)
    {
        _context.Users.Update(user);

        // Update the cache
        if (int.TryParse(user.Id, out var userId))
        {
            _userCache[userId] = user;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}