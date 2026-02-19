using FlowIQ.Application.DTOs.Common;
using FlowIQ.Application.DTOs.Expense;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowIQ.API.Controllers;

[ApiController]
[Route("api/business/{businessId:guid}/[controller]")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>
    /// Add a new expense entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] CreateExpenseRequest request)
    {
        var result = await _expenseService.CreateAsync(businessId, request);
        return CreatedAtAction(nameof(GetById), new { businessId, id = result.Id },
            ApiResponse<ExpenseResponse>.Ok(result, "Expense recorded."));
    }

    /// <summary>
    /// Update an existing expense entry.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid businessId, Guid id, [FromBody] UpdateExpenseRequest request)
    {
        var result = await _expenseService.UpdateAsync(id, businessId, request);
        return Ok(ApiResponse<ExpenseResponse>.Ok(result, "Expense updated."));
    }

    /// <summary>
    /// Delete an expense entry.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid businessId, Guid id)
    {
        await _expenseService.DeleteAsync(id, businessId);
        return NoContent();
    }

    /// <summary>
    /// Get an expense entry by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid businessId, Guid id)
    {
        var result = await _expenseService.GetByIdAsync(id, businessId);
        return Ok(ApiResponse<ExpenseResponse>.Ok(result!));
    }

    /// <summary>
    /// Get expenses by date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDateRange(Guid businessId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _expenseService.GetByDateRangeAsync(businessId, from, to);
        return Ok(ApiResponse<IEnumerable<ExpenseResponse>>.Ok(result));
    }

    /// <summary>
    /// Get expenses by category.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(Guid businessId, ExpenseCategory category)
    {
        var result = await _expenseService.GetByCategoryAsync(businessId, category);
        return Ok(ApiResponse<IEnumerable<ExpenseResponse>>.Ok(result));
    }
}
