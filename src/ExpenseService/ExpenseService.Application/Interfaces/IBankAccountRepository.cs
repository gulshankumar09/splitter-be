using ExpenseService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface IBankAccountRepository
{
    Task<BankAccount> GetByIdAsync(int id);
    Task<IEnumerable<BankAccount>> GetByUserIdAsync(int userId);
    Task<IEnumerable<BankAccount>> GetActiveAccountsAsync();
    Task AddAsync(BankAccount bankAccount);
    void Update(BankAccount bankAccount);
    Task SaveChangesAsync();

    // Transaction tracking
    Task<IEnumerable<string>> GetImportedTransactionIdsAsync(int bankAccountId);
    Task<IEnumerable<BankTransaction>> GetPendingTransactionsAsync(int bankAccountId);
    Task AddImportedTransactionAsync(int bankAccountId, string externalTransactionId);
    Task LinkTransactionToExpenseAsync(string externalTransactionId, int expenseId);
    Task UpdateTransactionStatusAsync(int bankAccountId, string externalTransactionId, bool isPending, decimal finalAmount);
}