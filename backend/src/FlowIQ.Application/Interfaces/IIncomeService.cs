using FlowIQ.Application.DTOs.Income;

namespace FlowIQ.Application.Interfaces;

public interface IIncomeService
{
    Task<IncomeResponse> CreateAsync(Guid businessId, CreateIncomeRequest request);
    Task<IncomeResponse> UpdateAsync(Guid incomeId, Guid businessId, UpdateIncomeRequest request);
    Task DeleteAsync(Guid incomeId, Guid businessId);
    Task<IncomeResponse?> GetByIdAsync(Guid incomeId, Guid businessId);
    Task<IEnumerable<IncomeResponse>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to);
}
