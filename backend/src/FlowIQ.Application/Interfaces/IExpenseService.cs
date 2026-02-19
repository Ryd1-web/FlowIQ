using FlowIQ.Application.DTOs.Expense;
using FlowIQ.Domain.Enums;

namespace FlowIQ.Application.Interfaces;

public interface IExpenseService
{
    Task<ExpenseResponse> CreateAsync(Guid businessId, CreateExpenseRequest request);
    Task<ExpenseResponse> UpdateAsync(Guid expenseId, Guid businessId, UpdateExpenseRequest request);
    Task DeleteAsync(Guid expenseId, Guid businessId);
    Task<ExpenseResponse?> GetByIdAsync(Guid expenseId, Guid businessId);
    Task<IEnumerable<ExpenseResponse>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to);
    Task<IEnumerable<ExpenseResponse>> GetByCategoryAsync(Guid businessId, ExpenseCategory category);
}
