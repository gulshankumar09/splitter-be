using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class BudgetCategory : BaseEntity
{
    public ExpenseCategory Category { get; private set; }
    public decimal? LimitAmount { get; private set; }

    // For EF Core
    private BudgetCategory() { }

    public BudgetCategory(ExpenseCategory category, decimal? limitAmount = null)
    {
        Category = category;
        LimitAmount = limitAmount;
    }

    public void UpdateLimit(decimal? limitAmount)
    {
        LimitAmount = limitAmount;
    }
}