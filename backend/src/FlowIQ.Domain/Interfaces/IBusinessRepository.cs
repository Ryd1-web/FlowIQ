using FlowIQ.Domain.Entities;

namespace FlowIQ.Domain.Interfaces;

public interface IBusinessRepository : IRepository<Business>
{
    Task<IEnumerable<Business>> GetByUserIdAsync(Guid userId);
}
