using FlowIQ.API.Extensions;
using FlowIQ.Application.DTOs.Business;
using FlowIQ.Application.DTOs.Common;
using FlowIQ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowIQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    /// <summary>
    /// Create a new business profile.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BusinessResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request)
    {
        var userId = User.GetUserId();
        var result = await _businessService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BusinessResponse>.Ok(result, "Business created."));
    }

    /// <summary>
    /// Update an existing business profile.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BusinessResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusinessRequest request)
    {
        var userId = User.GetUserId();
        var result = await _businessService.UpdateAsync(id, userId, request);
        return Ok(ApiResponse<BusinessResponse>.Ok(result, "Business updated."));
    }

    /// <summary>
    /// Get a business by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BusinessResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var result = await _businessService.GetByIdAsync(id, userId);
        return Ok(ApiResponse<BusinessResponse>.Ok(result!));
    }

    /// <summary>
    /// Get all businesses for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BusinessResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        var result = await _businessService.GetByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<BusinessResponse>>.Ok(result));
    }
}
