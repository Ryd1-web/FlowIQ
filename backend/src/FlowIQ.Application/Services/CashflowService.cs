using FlowIQ.Application.DTOs.Cashflow;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Enums;
using FlowIQ.Domain.Interfaces;

namespace FlowIQ.Application.Services;

public class CashflowService : ICashflowService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IBusinessRepository _businessRepository;

    public CashflowService(
        IIncomeRepository incomeRepository,
        IExpenseRepository expenseRepository,
        IBusinessRepository businessRepository)
    {
        _incomeRepository = incomeRepository;
        _expenseRepository = expenseRepository;
        _businessRepository = businessRepository;
    }

    public async Task<CashflowResponse> CalculateCashflowAsync(CashflowRequest request)
    {
        var totalIncome = await _incomeRepository.GetTotalByDateRangeAsync(request.BusinessId, request.From, request.To);
        var totalExpense = await _expenseRepository.GetTotalByDateRangeAsync(request.BusinessId, request.From, request.To);
        var net = totalIncome - totalExpense;
        var status = DetermineStatus(totalIncome, totalExpense, net);

        return new CashflowResponse(
            totalIncome, totalExpense, net,
            status, status.ToString(),
            request.From, request.To);
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid businessId)
    {
        var business = await _businessRepository.GetByIdAsync(businessId)
            ?? throw new NotFoundException("Business", businessId);

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todayIncome = await _incomeRepository.GetTotalByDateRangeAsync(businessId, today, tomorrow);
        var todayExpense = await _expenseRepository.GetTotalByDateRangeAsync(businessId, today, tomorrow);
        var todayNet = todayIncome - todayExpense;
        var todayStatus = DetermineStatus(todayIncome, todayExpense, todayNet);

        var weekIncome = await _incomeRepository.GetTotalByDateRangeAsync(businessId, weekStart, tomorrow);
        var weekExpense = await _expenseRepository.GetTotalByDateRangeAsync(businessId, weekStart, tomorrow);

        var monthIncome = await _incomeRepository.GetTotalByDateRangeAsync(businessId, monthStart, tomorrow);
        var monthExpense = await _expenseRepository.GetTotalByDateRangeAsync(businessId, monthStart, tomorrow);

        return new DashboardSummaryResponse(
            todayIncome, todayExpense, todayNet,
            todayStatus, todayStatus.ToString(),
            weekIncome, weekExpense,
            monthIncome, monthExpense,
            business.Name);
    }

    public async Task<CashflowTrendResponse> GetTrendsAsync(Guid businessId, string period)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        DateTime from;
        int days;

        switch (period.ToLower())
        {
            case "weekly":
                from = today.AddDays(-6);
                days = 7;
                break;
            case "monthly":
                from = today.AddDays(-29);
                days = 30;
                break;
            default: // daily — last 7 days
                from = today.AddDays(-6);
                days = 7;
                break;
        }

        var trends = new List<CashflowTrendItem>();

        for (int i = 0; i < days; i++)
        {
            var dayStart = from.AddDays(i);
            var dayEnd = dayStart.AddDays(1);

            var income = await _incomeRepository.GetTotalByDateRangeAsync(businessId, dayStart, dayEnd);
            var expense = await _expenseRepository.GetTotalByDateRangeAsync(businessId, dayStart, dayEnd);

            trends.Add(new CashflowTrendItem(dayStart, income, expense, income - expense));
        }

        return new CashflowTrendResponse(trends, period);
    }

    public async Task<CashflowHealthResponse> GetHealthAsync(Guid businessId)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var tomorrow = today.AddDays(1);

        var monthIncome = await _incomeRepository.GetTotalByDateRangeAsync(businessId, monthStart, tomorrow);
        var monthExpense = await _expenseRepository.GetTotalByDateRangeAsync(businessId, monthStart, tomorrow);
        var net = monthIncome - monthExpense;
        var status = DetermineStatus(monthIncome, monthExpense, net);
        var ratio = monthExpense > 0 ? Math.Round(monthIncome / monthExpense, 2) : monthIncome > 0 ? 999m : 0m;

        var message = status switch
        {
            CashflowStatus.Healthy => "Your business cashflow is looking good! Keep it up.",
            CashflowStatus.Warning => "Your expenses are getting close to your income. Watch your spending.",
            CashflowStatus.Critical => "Your expenses have exceeded your income. Take action now.",
            _ => "Unable to determine cashflow health."
        };

        return new CashflowHealthResponse(status, status.ToString(), message, net, ratio);
    }

    private static CashflowStatus DetermineStatus(decimal income, decimal expense, decimal net)
    {
        if (income == 0 && expense == 0)
            return CashflowStatus.Healthy;

        if (net < 0)
            return CashflowStatus.Critical;

        // If expenses are more than 80% of income
        if (income > 0 && expense / income > 0.8m)
            return CashflowStatus.Warning;

        return CashflowStatus.Healthy;
    }
}

