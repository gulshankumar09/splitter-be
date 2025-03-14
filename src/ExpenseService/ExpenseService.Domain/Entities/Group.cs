using SharedLibrary.Domain;
using System.Collections.Generic;

namespace ExpenseService.Domain.Entities;

public class Group : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int OwnerId { get; private set; }
    public GroupType Type { get; private set; }
    public List<GroupMember> Members { get; private set; }
    public List<Expense> Expenses { get; private set; }
    public string? ImageUrl { get; private set; }

    // For EF Core
    private Group() { }

    public Group(string name, int ownerId, GroupType type, string? description = null)
        : base()
    {
        Name = name;
        OwnerId = ownerId;
        Type = type;
        Description = description;
        Members = new List<GroupMember>();
        Expenses = new List<Expense>();

        // Add owner as a member automatically
        AddMember(ownerId, GroupMemberRole.Owner);
    }

    public void UpdateDetails(string name, GroupType type, string? description = null)
    {
        Name = name;
        Type = type;
        Description = description;
    }

    public void SetImageUrl(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void AddMember(int userId, GroupMemberRole role = GroupMemberRole.Member)
    {
        if (!IsMember(userId))
        {
            var member = new GroupMember(userId, role);
            Members.Add(member);
        }
    }

    public void RemoveMember(int userId)
    {
        if (userId == OwnerId)
        {
            throw new InvalidOperationException("Cannot remove the owner from the group");
        }

        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            Members.Remove(member);
        }
    }

    public bool IsMember(int userId)
    {
        return Members.Any(m => m.UserId == userId);
    }

    public void UpdateMemberRole(int userId, GroupMemberRole role)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            member.UpdateRole(role);
        }
    }

    public void AddExpense(Expense expense)
    {
        if (!Expenses.Contains(expense))
        {
            Expenses.Add(expense);
        }
    }

    public GroupBalanceSummary GenerateBalanceSummary()
    {
        var balances = new Dictionary<int, decimal>();
        var transactions = new List<BalanceTransaction>();

        // Initialize balances for all members
        foreach (var member in Members)
        {
            balances[member.UserId] = 0;
        }

        // Calculate what each person has paid and what they owe
        foreach (var expense in Expenses)
        {
            // Add what the payer has paid
            if (balances.ContainsKey(expense.PaidByUserId))
            {
                balances[expense.PaidByUserId] += expense.Amount;
            }

            // Subtract what each person owes
            foreach (var split in expense.Splits)
            {
                if (balances.ContainsKey(split.UserId))
                {
                    balances[split.UserId] -= split.Amount;
                }
            }
        }

        // Generate simplified transactions to settle debts
        var debtors = balances.Where(b => b.Value < 0)
            .OrderBy(b => b.Value)
            .ToList();
        var creditors = balances.Where(b => b.Value > 0)
            .OrderByDescending(b => b.Value)
            .ToList();

        int debtorIndex = 0;
        int creditorIndex = 0;

        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];

            var debtAmount = Math.Min(Math.Abs(debtor.Value), creditor.Value);

            if (debtAmount > 0)
            {
                transactions.Add(new BalanceTransaction
                {
                    FromUserId = debtor.Key,
                    ToUserId = creditor.Key,
                    Amount = debtAmount
                });
            }

            // Update balances
            balances[debtor.Key] += debtAmount;
            balances[creditor.Key] -= debtAmount;

            // Move to next debtor/creditor if their balance is cleared
            if (Math.Abs(balances[debtor.Key]) < 0.01m)
            {
                debtorIndex++;
            }

            if (Math.Abs(balances[creditor.Key]) < 0.01m)
            {
                creditorIndex++;
            }
        }

        return new GroupBalanceSummary
        {
            GroupId = Id,
            MemberBalances = balances.Select(b => new MemberBalance
            {
                UserId = b.Key,
                Balance = b.Value
            }).ToList(),
            SettlementTransactions = transactions
        };
    }
}

public enum GroupType
{
    Trip,
    Roommates,
    Event,
    Project,
    Family,
    Other
}

public class GroupBalanceSummary
{
    public int GroupId { get; set; }
    public List<MemberBalance> MemberBalances { get; set; } = new List<MemberBalance>();
    public List<BalanceTransaction> SettlementTransactions { get; set; } = new List<BalanceTransaction>();
}

public class MemberBalance
{
    public int UserId { get; set; }
    public decimal Balance { get; set; }
}

public class BalanceTransaction
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public decimal Amount { get; set; }
}