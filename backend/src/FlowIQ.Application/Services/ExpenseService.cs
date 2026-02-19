using FlowIQ.Application.DTOs.Expense;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Enums;
using FlowIQ.Domain.Interfaces;

namespace FlowIQ.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IBusinessRepository _businessRepository;

    public ExpenseService(IExpenseRepository expenseRepository, IBusinessRepository businessRepository)
    {
        _expenseRepository = expenseRepository;
        _businessRepository = businessRepository;
    }

    public async Task<ExpenseResponse> CreateAsync(Guid businessId, CreateExpenseRequest request)
    {
        if (!await _businessRepository.ExistsAsync(businessId))
            throw new NotFoundException("Business", businessId);

        var expense = new Expense
        {
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            TransactionDate = request.TransactionDate,
            Notes = request.Notes,
            BusinessId = businessId
        };

        await _expenseRepository.AddAsync(expense);
        return MapToResponse(expense);
    }

    public async Task<ExpenseResponse> UpdateAsync(Guid expenseId, Guid businessId, UpdateExpenseRequest request)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.BusinessId != businessId)
            throw new UnauthorizedException();

        expense.Amount = request.Amount;
        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.TransactionDate = request.TransactionDate;
        expense.Notes = request.Notes;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepository.UpdateAsync(expense);
        return MapToResponse(expense);
    }

    public async Task DeleteAsync(Guid expenseId, Guid businessId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.BusinessId != businessId)
            throw new UnauthorizedException();

        await _expenseRepository.DeleteAsync(expense);
    }

    public async Task<ExpenseResponse?> GetByIdAsync(Guid expenseId, Guid businessId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.BusinessId != businessId)
            throw new UnauthorizedException();

        return MapToResponse(expense);
    }

    public async Task<IEnumerable<ExpenseResponse>> GetByDateRangeAsync(Guid businessId, DateTime from, DateTime to)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(businessId, from, to);
        return expenses.Select(MapToResponse);
    }

    public async Task<IEnumerable<ExpenseResponse>> GetByCategoryAsync(Guid businessId, ExpenseCategory category)
    {
        var expenses = await _expenseRepository.GetByCategoryAsync(businessId, category);
        return expenses.Select(MapToResponse);
    }

    private static ExpenseResponse MapToResponse(Expense e)
        => new(e.Id, e.Amount, e.Category, e.Category.ToString(), e.Description,
               e.TransactionDate, e.Notes, e.BusinessId, e.CreatedAt);
}
