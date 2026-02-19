using FlowIQ.Application.DTOs.Common;
using FlowIQ.Application.DTOs.Income;
using FlowIQ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowIQ.API.Controllers;

[ApiController]
[Route("api/business/{businessId:guid}/[controller]")]
[Authorize]
public class IncomeController : ControllerBase
{
    private readonly IIncomeService _incomeService;

    public IncomeController(IIncomeService incomeService)
    {
        _incomeService = incomeService;
    }

    /// <summary>
    /// Add a new income entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] CreateIncomeRequest request)
    {
        var result = await _incomeService.CreateAsync(businessId, request);
        return CreatedAtAction(nameof(GetById), new { businessId, id = result.Id },
            ApiResponse<IncomeResponse>.Ok(result, "Income recorded."));
    }

    /// <summary>
    /// Update an existing income entry.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid businessId, Guid id, [FromBody] UpdateIncomeRequest request)
    {
        var result = await _incomeService.UpdateAsync(id, businessId, request);
        return Ok(ApiResponse<IncomeResponse>.Ok(result, "Income updated."));
    }

    /// <summary>
    /// Delete an income entry.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid businessId, Guid id)
    {
        await _incomeService.DeleteAsync(id, businessId);
        return NoContent();
    }

    /// <summary>
    /// Get an income entry by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid businessId, Guid id)
    {
        var result = await _incomeService.GetByIdAsync(id, businessId);
        return Ok(ApiResponse<IncomeResponse>.Ok(result!));
    }

    /// <summary>
    /// Get incomes by date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncomeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDateRange(Guid businessId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _incomeService.GetByDateRangeAsync(businessId, from, to);
        return Ok(ApiResponse<IEnumerable<IncomeResponse>>.Ok(result));
    }
}
