using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Services;

namespace ExpenseService.Application.Interfaces;

public interface ISettlementRepository
{
    Task<SettlementRecord?> GetByIdAsync(int id);
    Task<IEnumerable<SettlementRecord>> GetAllAsync();
    Task<IEnumerable<SettlementRecord>> GetByUserIdAsync(int userId, bool isDebtor = true);
    Task<IEnumerable<SettlementRecord>> GetByStatusAsync(TransactionStatus status);
    Task<IEnumerable<SettlementRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(SettlementRecord settlement);
    void Update(SettlementRecord settlement);
    Task SaveChangesAsync();
}