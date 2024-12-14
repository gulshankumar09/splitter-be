using ExpenseService.Domain.Entities;
using SharedLibrary.Repositories;

namespace ExpenseService.Infrastructure.Repositories;

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ExpenseDbContext context) : base(context)
    {
    }
} 