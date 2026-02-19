using FlowIQ.Domain.Enums;

namespace FlowIQ.Application.DTOs.Expense;

public record CreateExpenseRequest(
    decimal Amount,
    ExpenseCategory Category,
    string Description,
    DateTime TransactionDate,
    string? Notes);

public record UpdateExpenseRequest(
    decimal Amount,
    ExpenseCategory Category,
    string Description,
    DateTime TransactionDate,
    string? Notes);

public record ExpenseResponse(
    Guid Id,
    decimal Amount,
    ExpenseCategory Category,
    string CategoryName,
    string Description,
    DateTime TransactionDate,
    string? Notes,
    Guid BusinessId,
    DateTime CreatedAt);
