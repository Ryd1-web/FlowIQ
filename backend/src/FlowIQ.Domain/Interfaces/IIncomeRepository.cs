using FlowIQ.Domain.Entities;

namespace FlowIQ.Domain.Interfaces;

public interface IIncomeRepository : IRepository<Income>
{
    Task<IEnumerable<Income>> GetByBusinessIdAsync(Guid businessId);
    Task<IEnumerable<Income>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to);
    Task<decimal> GetTotalByDateRangeAsync(Guid businessId, DateTime from, DateTime to);
}
