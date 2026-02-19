namespace FlowIQ.Application.DTOs.Auth;

public record RegisterRequest(string PhoneNumber, string FullName);

public record OtpRequest(string PhoneNumber);

public record VerifyOtpRequest(string PhoneNumber, string OtpCode);

public record AuthResponse(string Token, string FullName, string PhoneNumber, DateTime ExpiresAt);

public record OtpResponse(string Message, string PhoneNumber);

public record UpdateUserProfileRequest(string? PhoneNumber, string? FullName);

public record UserProfileResponse(Guid Id, string PhoneNumber, string FullName, bool IsVerified);
