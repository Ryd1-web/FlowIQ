using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;
using FlowIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowIQ.Infrastructure.Repositories;

public class BusinessRepository : Repository<Business>, IBusinessRepository
{
    public BusinessRepository(FlowIQDbContext context) : base(context) { }

    public async Task<IEnumerable<Business>> GetByUserIdAsync(Guid userId)
        => await _dbSet.Where(b => b.UserId == userId).ToListAsync();
}
