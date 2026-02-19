using FlowIQ.Domain.Enums;

namespace FlowIQ.Application.DTOs.Cashflow;

public record CashflowRequest(Guid BusinessId, DateTime From, DateTime To);

public record CashflowResponse(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetCashflow,
    CashflowStatus Status,
    string StatusLabel,
    DateTime From,
    DateTime To);

public record DashboardSummaryResponse(
    decimal TodayIncome,
    decimal TodayExpense,
    decimal TodayNetCashflow,
    CashflowStatus TodayStatus,
    string TodayStatusLabel,
    decimal WeekIncome,
    decimal WeekExpense,
    decimal MonthIncome,
    decimal MonthExpense,
    string BusinessName);

public record CashflowTrendItem(DateTime Date, decimal Income, decimal Expense, decimal Net);

public record CashflowTrendResponse(IEnumerable<CashflowTrendItem> Trends, string Period);

public record CashflowHealthResponse(
    CashflowStatus Status,
    string StatusLabel,
    string Message,
    decimal NetCashflow,
    decimal IncomeToExpenseRatio);
