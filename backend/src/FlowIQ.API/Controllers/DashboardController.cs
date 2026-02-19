using FlowIQ.Application.DTOs.Cashflow;
using FlowIQ.Application.DTOs.Common;
using FlowIQ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowIQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ICashflowService _cashflowService;

    public DashboardController(ICashflowService cashflowService)
    {
        _cashflowService = cashflowService;
    }

    /// <summary>
    /// Get dashboard summary — today's income, expense, net, and weekly/monthly totals.
    /// </summary>
    [HttpGet("summary/{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(Guid businessId)
    {
        var result = await _cashflowService.GetDashboardSummaryAsync(businessId);
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result));
    }

    /// <summary>
    /// Get cashflow trends — daily breakdown over a period.
    /// </summary>
    [HttpGet("trends/{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CashflowTrendResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrends(Guid businessId, [FromQuery] string period = "weekly")
    {
        var result = await _cashflowService.GetTrendsAsync(businessId, period);
        return Ok(ApiResponse<CashflowTrendResponse>.Ok(result));
    }

    /// <summary>
    /// Get cashflow health status.
    /// </summary>
    [HttpGet("health/{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CashflowHealthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth(Guid businessId)
    {
        var result = await _cashflowService.GetHealthAsync(businessId);
        return Ok(ApiResponse<CashflowHealthResponse>.Ok(result));
    }

    /// <summary>
    /// Calculate cashflow for a custom date range.
    /// </summary>
    [HttpPost("cashflow")]
    [ProducesResponseType(typeof(ApiResponse<CashflowResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateCashflow([FromBody] CashflowRequest request)
    {
        var result = await _cashflowService.CalculateCashflowAsync(request);
        return Ok(ApiResponse<CashflowResponse>.Ok(result));
    }
}
