using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Enums;

namespace FlowIQ.Domain.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetByBusinessIdAsync(Guid businessId);
    Task<IEnumerable<Expense>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to);
    Task<IEnumerable<Expense>> GetByCategoryAsync(Guid businessId, ExpenseCategory category);
    Task<decimal> GetTotalByDateRangeAsync(Guid businessId, DateTime from, DateTime to);
}
