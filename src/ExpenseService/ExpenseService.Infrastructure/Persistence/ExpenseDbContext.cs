using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Infrastructure.Persistence;

public class ExpenseDbContext : DbContext
{
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExpenseSplit> ExpenseSplits { get; set; }
    public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<SettlementRecord> SettlementRecords { get; set; }

    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseSplitConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new GroupConfiguration());
        modelBuilder.ApplyConfiguration(new GroupMemberConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementRecordConfiguration());
    }
}