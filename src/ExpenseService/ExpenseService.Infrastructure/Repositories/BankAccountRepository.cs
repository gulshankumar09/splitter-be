using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Infrastructure.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ExpenseDbContext _context;

    public BankAccountRepository(ExpenseDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount> GetByIdAsync(int id)
    {
        return await _context.BankAccounts.FindAsync(id);
    }

    public async Task<IEnumerable<BankAccount>> GetByUserIdAsync(int userId)
    {
        return await _context.BankAccounts
            .Where(ba => ba.UserId == userId)
            .OrderByDescending(ba => ba.LastSyncedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BankAccount>> GetActiveAccountsAsync()
    {
        return await _context.BankAccounts
            .Where(ba => ba.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(BankAccount bankAccount)
    {
        await _context.BankAccounts.AddAsync(bankAccount);
    }

    public void Update(BankAccount bankAccount)
    {
        _context.BankAccounts.Update(bankAccount);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetImportedTransactionIdsAsync(int bankAccountId)
    {
        return await _context.BankTransactions
            .Where(bt => bt.BankAccountId == bankAccountId)
            .Select(bt => bt.ExternalId)
            .ToListAsync();
    }

    public async Task<IEnumerable<BankTransaction>> GetPendingTransactionsAsync(int bankAccountId)
    {
        return await _context.BankTransactions
            .Where(bt => bt.BankAccountId == bankAccountId && bt.IsPending)
            .ToListAsync();
    }

    public async Task AddImportedTransactionAsync(int bankAccountId, string externalTransactionId)
    {
        // Check if transaction already exists
        var existingTransaction = await _context.BankTransactions
            .FirstOrDefaultAsync(bt => bt.ExternalId == externalTransactionId);

        if (existingTransaction == null)
        {
            // Create a placeholder transaction - the details will be filled in later
            var transaction = new BankTransaction(
                bankAccountId,
                externalTransactionId,
                DateTime.UtcNow,
                0,
                "Pending Transaction",
                "Unknown",
                "Unknown",
                true);

            await _context.BankTransactions.AddAsync(transaction);
        }
    }

    public async Task LinkTransactionToExpenseAsync(string externalTransactionId, int expenseId)
    {
        var transaction = await _context.BankTransactions
            .FirstOrDefaultAsync(bt => bt.ExternalId == externalTransactionId);

        if (transaction != null)
        {
            transaction.LinkToExpense(expenseId);
        }
    }

    public async Task UpdateTransactionStatusAsync(int bankAccountId, string externalTransactionId, bool isPending, decimal finalAmount)
    {
        var transaction = await _context.BankTransactions
            .FirstOrDefaultAsync(bt => bt.BankAccountId == bankAccountId && bt.ExternalId == externalTransactionId);

        if (transaction != null)
        {
            transaction.UpdatePendingStatus(isPending, finalAmount);
        }
    }
}