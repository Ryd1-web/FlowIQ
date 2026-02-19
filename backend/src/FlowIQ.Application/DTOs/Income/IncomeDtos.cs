namespace FlowIQ.Application.DTOs.Income;

public record CreateIncomeRequest(decimal Amount, string Source, DateTime TransactionDate, string? Notes);

public record UpdateIncomeRequest(decimal Amount, string Source, DateTime TransactionDate, string? Notes);

public record IncomeResponse(
    Guid Id,
    decimal Amount,
    string Source,
    DateTime TransactionDate,
    string? Notes,
    Guid BusinessId,
    DateTime CreatedAt);
