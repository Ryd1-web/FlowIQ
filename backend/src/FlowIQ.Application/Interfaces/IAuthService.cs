using FlowIQ.Application.DTOs.Auth;

namespace FlowIQ.Application.Interfaces;

public interface IAuthService
{
    Task<OtpResponse> RegisterAsync(RegisterRequest request);
    Task<OtpResponse> RequestOtpAsync(string phoneNumber);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
}
