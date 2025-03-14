using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Services;
using SharedLibrary.Models;

namespace ExpenseService.Application.Services;

public class BalanceService : IBalanceService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ISettlementRepository _settlementRepository;

    public BalanceService(
        IExpenseRepository expenseRepository,
        ISettlementRepository settlementRepository)
    {
        _expenseRepository = expenseRepository;
        _settlementRepository = settlementRepository;
    }

    public async Task<Result<GlobalBalanceSummaryDto>> GetGlobalBalanceSummaryAsync()
    {
        try
        {
            var expenses = await _expenseRepository.GetAllAsync();
            var balanceSummary = BalanceCalculator.CalculateGlobalBalances(expenses);

            return Result<GlobalBalanceSummaryDto>.Success(MapToDto(balanceSummary));
        }
        catch (Exception ex)
        {
            return Result<GlobalBalanceSummaryDto>.Failure($"Failed to calculate global balance summary: {ex.Message}");
        }
    }

    public async Task<Result<GlobalBalanceSummaryDto>> GetUserBalanceSummaryAsync(UserBalanceQueryDto query)
    {
        try
        {
            IEnumerable<Expense> expenses;

            // Get expenses related to the specified user
            if (query.StartDate.HasValue && query.EndDate.HasValue)
            {
                expenses = await _expenseRepository.GetByDateRangeAsync(query.StartDate.Value, query.EndDate.Value);
            }
            else
            {
                expenses = await _expenseRepository.GetAllAsync();
            }

            // Filter expenses to only include those where the user is involved
            expenses = expenses.Where(e =>
                e.PaidByUserId == query.UserId ||
                e.Splits.Any(s => s.UserId == query.UserId));

            // Exclude settled expenses if requested
            if (!query.IncludeSettled)
            {
                var settlements = await _settlementRepository.GetByUserIdAsync(query.UserId);
                var settledAmounts = new Dictionary<(int, int), decimal>();

                foreach (var settlement in settlements.Where(s => s.Status == TransactionStatus.Completed))
                {
                    var key = (settlement.FromUserId, settlement.ToUserId);
                    if (!settledAmounts.ContainsKey(key))
                    {
                        settledAmounts[key] = 0;
                    }
                    settledAmounts[key] += settlement.Amount;
                }

                // TODO: Apply settlement logic to the expenses
                // This would be complex and would require tracking which parts of which expenses have been settled
                // For now, we'll just return the raw summary
            }

            var balanceSummary = BalanceCalculator.CalculateGlobalBalances(expenses);

            return Result<GlobalBalanceSummaryDto>.Success(MapToDto(balanceSummary));
        }
        catch (Exception ex)
        {
            return Result<GlobalBalanceSummaryDto>.Failure($"Failed to calculate user balance summary: {ex.Message}");
        }
    }

    public async Task<Result<List<SettlementTransactionDto>>> GetSettlementPlanAsync(int userId)
    {
        try
        {
            // Get all expenses
            var expenses = await _expenseRepository.GetAllAsync();

            // Filter expenses to only include those where the user is involved
            expenses = expenses.Where(e =>
                e.PaidByUserId == userId ||
                e.Splits.Any(s => s.UserId == userId));

            // Calculate balances
            var balanceSummary = BalanceCalculator.CalculateGlobalBalances(expenses);

            // Filter settlements to only include those involving the user
            var userSettlements = balanceSummary.SettlementTransactions
                .Where(t => t.FromUserId == userId || t.ToUserId == userId)
                .ToList();

            var settlementDtos = userSettlements.Select(s => new SettlementTransactionDto
            {
                FromUserId = s.FromUserId,
                ToUserId = s.ToUserId,
                Amount = s.Amount,
                Status = s.Status,
                CompletedAt = s.CompletedAt
            }).ToList();

            return Result<List<SettlementTransactionDto>>.Success(settlementDtos);
        }
        catch (Exception ex)
        {
            return Result<List<SettlementTransactionDto>>.Failure($"Failed to generate settlement plan: {ex.Message}");
        }
    }

    public async Task<Result<SettlementTransactionDto>> RecordSettlementAsync(RecordSettlementRequest request)
    {
        try
        {
            // Create a settlement record
            var settlement = new SettlementRecord(
                request.FromUserId,
                request.ToUserId,
                request.Amount);

            await _settlementRepository.AddAsync(settlement);
            await _settlementRepository.SaveChangesAsync();

            var dto = new SettlementTransactionDto
            {
                FromUserId = settlement.FromUserId,
                ToUserId = settlement.ToUserId,
                Amount = settlement.Amount,
                Status = settlement.Status,
                CompletedAt = settlement.CompletedAt
            };

            return Result<SettlementTransactionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SettlementTransactionDto>.Failure($"Failed to record settlement: {ex.Message}");
        }
    }

    public async Task<Result<SettlementTransactionDto>> MarkSettlementCompletedAsync(int settlementId)
    {
        try
        {
            var settlement = await _settlementRepository.GetByIdAsync(settlementId);
            if (settlement == null)
            {
                return Result<SettlementTransactionDto>.Failure("Settlement not found");
            }

            settlement.MarkAsCompleted();
            _settlementRepository.Update(settlement);
            await _settlementRepository.SaveChangesAsync();

            var dto = new SettlementTransactionDto
            {
                FromUserId = settlement.FromUserId,
                ToUserId = settlement.ToUserId,
                Amount = settlement.Amount,
                Status = settlement.Status,
                CompletedAt = settlement.CompletedAt
            };

            return Result<SettlementTransactionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SettlementTransactionDto>.Failure($"Failed to mark settlement as completed: {ex.Message}");
        }
    }

    public async Task<Result<SettlementTransactionDto>> CancelSettlementAsync(int settlementId)
    {
        try
        {
            var settlement = await _settlementRepository.GetByIdAsync(settlementId);
            if (settlement == null)
            {
                return Result<SettlementTransactionDto>.Failure("Settlement not found");
            }

            settlement.MarkAsCancelled();
            _settlementRepository.Update(settlement);
            await _settlementRepository.SaveChangesAsync();

            var dto = new SettlementTransactionDto
            {
                FromUserId = settlement.FromUserId,
                ToUserId = settlement.ToUserId,
                Amount = settlement.Amount,
                Status = settlement.Status,
                CompletedAt = settlement.CompletedAt
            };

            return Result<SettlementTransactionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SettlementTransactionDto>.Failure($"Failed to cancel settlement: {ex.Message}");
        }
    }

    private static GlobalBalanceSummaryDto MapToDto(GlobalBalanceSummary summary)
    {
        return new GlobalBalanceSummaryDto
        {
            UserBalances = summary.UserBalances.Select(ub => new UserBalanceDto
            {
                UserId = ub.UserId,
                NetBalance = ub.NetBalance,
                TotalExpenses = ub.TotalExpenses,
                TotalDebts = ub.TotalDebts
            }).ToList(),
            DebtRelationships = summary.DebtRelationships.Select(dr => new DebtRelationshipDto
            {
                DebtorId = dr.DebtorId,
                CreditorId = dr.CreditorId,
                Amount = dr.Amount
            }).ToList(),
            SettlementTransactions = summary.SettlementTransactions.Select(st => new SettlementTransactionDto
            {
                FromUserId = st.FromUserId,
                ToUserId = st.ToUserId,
                Amount = st.Amount,
                Status = st.Status,
                CompletedAt = st.CompletedAt
            }).ToList()
        };
    }
}