using FlowIQ.Application.DTOs.Cashflow;

namespace FlowIQ.Application.Interfaces;

public interface ICashflowService
{
    Task<CashflowResponse> CalculateCashflowAsync(CashflowRequest request);
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid businessId);
    Task<CashflowTrendResponse> GetTrendsAsync(Guid businessId, string period);
    Task<CashflowHealthResponse> GetHealthAsync(Guid businessId);
}
