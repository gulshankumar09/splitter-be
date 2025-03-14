using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Services;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories;

namespace ExpenseService.Infrastructure.Repositories;

public class SettlementRepository : GenericRepository<SettlementRecord>, ISettlementRepository
{
    private readonly ExpenseDbContext _context;

    public SettlementRepository(ExpenseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<SettlementRecord?> GetByIdAsync(int id)
    {
        return await _context.SettlementRecords
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SettlementRecord>> GetAllAsync()
    {
        return await _context.SettlementRecords.ToListAsync();
    }

    public async Task<IEnumerable<SettlementRecord>> GetByUserIdAsync(int userId, bool isDebtor = true)
    {
        if (isDebtor)
        {
            return await _context.SettlementRecords
                .Where(s => s.FromUserId == userId)
                .ToListAsync();
        }
        else
        {
            return await _context.SettlementRecords
                .Where(s => s.ToUserId == userId)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<SettlementRecord>> GetByStatusAsync(TransactionStatus status)
    {
        return await _context.SettlementRecords
            .Where(s => s.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<SettlementRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SettlementRecords
            .Where(s => (s.CompletedAt >= startDate && s.CompletedAt <= endDate) ||
                        (s.CreatedAt >= startDate && s.CreatedAt <= endDate))
            .ToListAsync();
    }

    public void Update(SettlementRecord settlement)
    {
        _context.SettlementRecords.Update(settlement);
    }
}