using ExpenseService.Domain.Entities;

namespace ExpenseService.Domain.Services;

public class BalanceCalculator
{
    public static GlobalBalanceSummary CalculateGlobalBalances(IEnumerable<Expense> expenses)
    {
        var balances = new Dictionary<int, decimal>();
        var userExpenses = new Dictionary<int, decimal>();
        var userDebts = new Dictionary<int, decimal>();
        var owesRelationships = new Dictionary<(int, int), decimal>();

        // First pass: calculate raw balances
        foreach (var expense in expenses)
        {
            // Initialize the payer if not already present
            if (!balances.ContainsKey(expense.PaidByUserId))
            {
                balances[expense.PaidByUserId] = 0;
                userExpenses[expense.PaidByUserId] = 0;
                userDebts[expense.PaidByUserId] = 0;
            }

            // Add to the payer's balance
            balances[expense.PaidByUserId] += expense.Amount;
            userExpenses[expense.PaidByUserId] += expense.Amount;

            // Process each split
            foreach (var split in expense.Splits)
            {
                // Initialize the user if not already present
                if (!balances.ContainsKey(split.UserId))
                {
                    balances[split.UserId] = 0;
                    userExpenses[split.UserId] = 0;
                    userDebts[split.UserId] = 0;
                }

                // Subtract from the user's balance
                balances[split.UserId] -= split.Amount;
                userDebts[split.UserId] += split.Amount;

                // Record direct debt relationship
                var key = (split.UserId, expense.PaidByUserId);
                if (!owesRelationships.ContainsKey(key))
                {
                    owesRelationships[key] = 0;
                }
                owesRelationships[key] += split.Amount;
            }
        }

        // Create detailed user balances
        var userBalances = balances.Select(b => new UserBalance
        {
            UserId = b.Key,
            NetBalance = b.Value,
            TotalExpenses = userExpenses.GetValueOrDefault(b.Key),
            TotalDebts = userDebts.GetValueOrDefault(b.Key)
        }).ToList();

        // Create detailed debt relationships
        var debtRelationships = owesRelationships
            .Select(o => new DebtRelationship
            {
                DebtorId = o.Key.Item1,
                CreditorId = o.Key.Item2,
                Amount = o.Value
            })
            .OrderByDescending(d => d.Amount)
            .ToList();

        // Calculate optimized settlement plan
        var settlementPlan = CalculateOptimizedSettlementPlan(balances);

        return new GlobalBalanceSummary
        {
            UserBalances = userBalances,
            DebtRelationships = debtRelationships,
            SettlementTransactions = settlementPlan
        };
    }

    private static List<SettlementTransaction> CalculateOptimizedSettlementPlan(Dictionary<int, decimal> balances)
    {
        var transactions = new List<SettlementTransaction>();

        // Separate debtors and creditors
        var debtors = balances.Where(b => b.Value < 0)
            .OrderBy(b => b.Value)
            .Select(b => new KeyValuePair<int, decimal>(b.Key, Math.Abs(b.Value)))
            .ToList();

        var creditors = balances.Where(b => b.Value > 0)
            .OrderByDescending(b => b.Value)
            .ToList();

        int debtorIndex = 0;
        int creditorIndex = 0;

        // Generate transactions until all balances are settled
        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];

            // Calculate the transfer amount
            decimal transferAmount = Math.Min(debtor.Value, creditor.Value);

            // Create the transaction if amount is significant
            if (transferAmount > 0.01m)
            {
                transactions.Add(new SettlementTransaction
                {
                    FromUserId = debtor.Key,
                    ToUserId = creditor.Key,
                    Amount = Math.Round(transferAmount, 2),
                    Status = TransactionStatus.Pending
                });
            }

            // Update remaining balances
            decimal remainingDebtorAmount = debtor.Value - transferAmount;
            decimal remainingCreditorAmount = creditor.Value - transferAmount;

            // Move to next debtor if their balance is settled
            if (remainingDebtorAmount < 0.01m)
            {
                debtorIndex++;
            }
            else
            {
                // Update the debtor's remaining balance
                debtors[debtorIndex] = new KeyValuePair<int, decimal>(debtor.Key, remainingDebtorAmount);
            }

            // Move to next creditor if their balance is settled
            if (remainingCreditorAmount < 0.01m)
            {
                creditorIndex++;
            }
            else
            {
                // Update the creditor's remaining balance
                creditors[creditorIndex] = new KeyValuePair<int, decimal>(creditor.Key, remainingCreditorAmount);
            }
        }

        return transactions;
    }
}

public class GlobalBalanceSummary
{
    public List<UserBalance> UserBalances { get; set; } = new();
    public List<DebtRelationship> DebtRelationships { get; set; } = new();
    public List<SettlementTransaction> SettlementTransactions { get; set; } = new();
}

public class UserBalance
{
    public int UserId { get; set; }
    public decimal NetBalance { get; set; } // Positive means owed money, negative means owes money
    public decimal TotalExpenses { get; set; } // Total amount paid
    public decimal TotalDebts { get; set; } // Total amount owed
}

public class DebtRelationship
{
    public int DebtorId { get; set; } // User who owes money
    public int CreditorId { get; set; } // User who is owed money
    public decimal Amount { get; set; } // Amount owed
}

public class SettlementTransaction
{
    public int FromUserId { get; set; } // User who pays
    public int ToUserId { get; set; } // User who receives
    public decimal Amount { get; set; } // Amount to transfer
    public TransactionStatus Status { get; set; } // Status of the transaction
    public DateTime? CompletedAt { get; set; } // When the transaction was completed
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Cancelled
}