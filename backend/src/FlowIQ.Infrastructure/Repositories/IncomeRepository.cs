using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;
using FlowIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowIQ.Infrastructure.Repositories;

public class IncomeRepository : Repository<Income>, IIncomeRepository
{
    public IncomeRepository(FlowIQDbContext context) : base(context) { }

    public async Task<IEnumerable<Income>> GetByBusinessIdAsync(Guid businessId)
        => await _dbSet.Where(i => i.BusinessId == businessId)
                       .OrderByDescending(i => i.TransactionDate)
                       .ToListAsync();

    public async Task<IEnumerable<Income>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to)
        => await _dbSet.Where(i => i.BusinessId == businessId
                                && i.TransactionDate >= from
                                && i.TransactionDate < to)
                       .OrderByDescending(i => i.TransactionDate)
                       .ToListAsync();

    public async Task<decimal> GetTotalByDateRangeAsync(Guid businessId, DateTime from, DateTime to)
        => await _dbSet.Where(i => i.BusinessId == businessId
                                && i.TransactionDate >= from
                                && i.TransactionDate < to)
                       .SumAsync(i => i.Amount);
}
