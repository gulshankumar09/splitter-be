using ExpenseService.Domain.Services;

namespace ExpenseService.Application.DTOs;

public class GlobalBalanceSummaryDto
{
    public List<UserBalanceDto> UserBalances { get; set; } = new();
    public List<DebtRelationshipDto> DebtRelationships { get; set; } = new();
    public List<SettlementTransactionDto> SettlementTransactions { get; set; } = new();
}

public class UserBalanceDto
{
    public int UserId { get; set; }
    public decimal NetBalance { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalDebts { get; set; }
}

public class DebtRelationshipDto
{
    public int DebtorId { get; set; }
    public int CreditorId { get; set; }
    public decimal Amount { get; set; }
}

public class SettlementTransactionDto
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class UserBalanceQueryDto
{
    public int UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeSettled { get; set; } = false;
}

public class RecordSettlementRequest
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public decimal Amount { get; set; }
}