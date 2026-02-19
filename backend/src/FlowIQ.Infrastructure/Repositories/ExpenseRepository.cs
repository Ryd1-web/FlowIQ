using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Enums;
using FlowIQ.Domain.Interfaces;
using FlowIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowIQ.Infrastructure.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(FlowIQDbContext context) : base(context) { }

    public async Task<IEnumerable<Expense>> GetByBusinessIdAsync(Guid businessId)
        => await _dbSet.Where(e => e.BusinessId == businessId)
                       .OrderByDescending(e => e.TransactionDate)
                       .ToListAsync();

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to)
        => await _dbSet.Where(e => e.BusinessId == businessId
                                && e.TransactionDate >= from
                                && e.TransactionDate < to)
                       .OrderByDescending(e => e.TransactionDate)
                       .ToListAsync();

    public async Task<IEnumerable<Expense>> GetByCategoryAsync(Guid businessId, ExpenseCategory category)
        => await _dbSet.Where(e => e.BusinessId == businessId && e.Category == category)
                       .OrderByDescending(e => e.TransactionDate)
                       .ToListAsync();

    public async Task<decimal> GetTotalByDateRangeAsync(Guid businessId, DateTime from, DateTime to)
        => await _dbSet.Where(e => e.BusinessId == businessId
                                && e.TransactionDate >= from
                                && e.TransactionDate < to)
                       .SumAsync(e => e.Amount);
}
