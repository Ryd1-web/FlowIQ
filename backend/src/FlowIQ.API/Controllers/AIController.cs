using FlowIQ.Application.DTOs.AI;
using FlowIQ.Application.DTOs.Common;
using FlowIQ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowIQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IAIServiceClient _aiClient;
    private readonly IIncomeService _incomeService;
    private readonly IExpenseService _expenseService;

    public AIController(IAIServiceClient aiClient, IIncomeService incomeService, IExpenseService expenseService)
    {
        _aiClient = aiClient;
        _incomeService = incomeService;
        _expenseService = expenseService;
    }

    /// <summary>
    /// Get AI cashflow prediction for a business based on recent history.
    /// </summary>
    [HttpGet("predict/{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CashflowPredictionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Predict(Guid businessId, [FromQuery] int lookbackDays = 60, [FromQuery] int predictionDays = 30)
    {
        var today = DateTime.UtcNow.Date;
        var to = today.AddDays(1);
        var from = today.AddDays(-Math.Max(7, lookbackDays));
        var normalizedPredictionDays = Math.Clamp(predictionDays, 7, 90);

        var incomes = (await _incomeService.GetByDateRangeAsync(businessId, from, to)).ToList();
        var expenses = (await _expenseService.GetByDateRangeAsync(businessId, from, to)).ToList();

        // Aggregate into daily buckets
        var days = new List<DailyDataPoint>();
        for (var d = from; d < to; d = d.AddDays(1))
        {
            var dayStart = d;
            var incomeSum = incomes.Where(i => i.TransactionDate.Date == dayStart).Sum(i => i.Amount);
            var expenseSum = expenses.Where(e => e.TransactionDate.Date == dayStart).Sum(e => e.Amount);
            days.Add(new DailyDataPoint(dayStart, incomeSum, expenseSum));
        }

        var request = new CashflowPredictionRequest(businessId, days, normalizedPredictionDays);

        var result = await _aiClient.PredictCashflowAsync(request);
        if (result == null)
            return Ok(ApiResponse<CashflowPredictionResponse>.Fail("AI service unavailable"));

        return Ok(ApiResponse<CashflowPredictionResponse>.Ok(result));
    }

    [HttpGet("anomaly/{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AnomalyDetectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DetectAnomalies(Guid businessId, [FromQuery] int lookbackDays = 60)
    {
        var today = DateTime.UtcNow.Date;
        var to = today.AddDays(1);
        var from = today.AddDays(-Math.Max(7, lookbackDays));

        var incomes = (await _incomeService.GetByDateRangeAsync(businessId, from, to)).ToList();
        var expenses = (await _expenseService.GetByDateRangeAsync(businessId, from, to)).ToList();

        var days = new List<DailyDataPoint>();
        for (var d = from; d < to; d = d.AddDays(1))
        {
            var dayStart = d;
            var incomeSum = incomes.Where(i => i.TransactionDate.Date == dayStart).Sum(i => i.Amount);
            var expenseSum = expenses.Where(e => e.TransactionDate.Date == dayStart).Sum(e => e.Amount);
            days.Add(new DailyDataPoint(dayStart, incomeSum, expenseSum));
        }

        var request = new AnomalyDetectionRequest(businessId, days);
        var result = await _aiClient.DetectAnomaliesAsync(request);
        if (result == null)
            return Ok(ApiResponse<AnomalyDetectionResponse>.Fail("AI service unavailable"));

        return Ok(ApiResponse<AnomalyDetectionResponse>.Ok(result));
    }

    [HttpPost("categorize/receipt")]
    [ProducesResponseType(typeof(ApiResponse<ReceiptCategorizationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CategorizeReceipt([FromBody] ReceiptCategorizationRequest request)
    {
        var result = await _aiClient.CategorizeReceiptAsync(request);
        if (result == null)
            return Ok(ApiResponse<ReceiptCategorizationResponse>.Fail("AI service unavailable"));

        return Ok(ApiResponse<ReceiptCategorizationResponse>.Ok(result));
    }
}
