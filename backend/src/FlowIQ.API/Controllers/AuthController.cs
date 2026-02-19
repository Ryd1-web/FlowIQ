using FlowIQ.Application.DTOs.Auth;
using FlowIQ.Application.DTOs.Common;
using FlowIQ.Application.Interfaces;
using FlowIQ.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowIQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user with phone number.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<OtpResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<OtpResponse>.Ok(result, "Registration successful. OTP sent."));
    }

    /// <summary>
    /// Request OTP for an existing user.
    /// </summary>
    [HttpPost("request-otp")]
    [ProducesResponseType(typeof(ApiResponse<OtpResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request)
    {
        var result = await _authService.RequestOtpAsync(request.PhoneNumber);
        return Ok(ApiResponse<OtpResponse>.Ok(result));
    }

    /// <summary>
    /// Verify OTP and receive JWT token.
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _authService.VerifyOtpAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    /// <summary>
    /// Update current user's profile details.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var userId = User.GetUserId();
        var result = await _authService.UpdateProfileAsync(userId, request);
        return Ok(ApiResponse<UserProfileResponse>.Ok(result, "Profile updated successfully."));
    }
}
