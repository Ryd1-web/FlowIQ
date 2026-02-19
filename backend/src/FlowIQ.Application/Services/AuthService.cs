using FlowIQ.Application.DTOs.Auth;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;

namespace FlowIQ.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<OtpResponse> RegisterAsync(RegisterRequest request)
    {
        var phone = request.PhoneNumber.Trim();
        var existingUser = await _userRepository.GetByPhoneNumberAsync(phone);
        if (existingUser != null)
            throw new ConflictException("A user with this phone number already exists.");

        var otp = GenerateMockOtp();
        var user = new User
        {
            PhoneNumber = phone,
            FullName = request.FullName,
            OtpCode = otp,
            OtpExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsVerified = false
        };

        await _userRepository.AddAsync(user);

        // In production, send OTP via SMS gateway
        return new OtpResponse($"OTP sent successfully. (Mock OTP: {otp})", phone);
    }

    public async Task<OtpResponse> RequestOtpAsync(string phoneNumber)
    {
        phoneNumber = phoneNumber.Trim();
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber)
            ?? throw new NotFoundException("User", phoneNumber);

        var otp = GenerateMockOtp();
        user.OtpCode = otp;
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        await _userRepository.UpdateAsync(user);

        return new OtpResponse($"OTP sent successfully. (Mock OTP: {otp})", phoneNumber);
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var phone = request.PhoneNumber.Trim();
        var user = await _userRepository.GetByPhoneNumberAsync(phone)
            ?? throw new NotFoundException("User", phone);

        if (user.OtpCode != request.OtpCode || user.OtpExpiresAt < DateTime.UtcNow)
            throw new BadRequestException("Invalid or expired OTP.");

        // Generate token FIRST — if this fails, OTP stays intact for retry
        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        user.IsVerified = true;
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new AuthResponse(token, user.FullName, user.PhoneNumber, expiresAt);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var hasPhone = !string.IsNullOrWhiteSpace(request.PhoneNumber);
        var hasName = !string.IsNullOrWhiteSpace(request.FullName);
        if (!hasPhone && !hasName)
            throw new BadRequestException("Provide at least one field to update.");

        if (hasPhone)
        {
            var newPhone = request.PhoneNumber!.Trim();
            var existing = await _userRepository.GetByPhoneNumberAsync(newPhone);
            if (existing != null && existing.Id != userId)
                throw new ConflictException("A user with this phone number already exists.");
            user.PhoneNumber = newPhone;
        }

        if (hasName)
            user.FullName = request.FullName!.Trim();

        await _userRepository.UpdateAsync(user);
        return new UserProfileResponse(user.Id, user.PhoneNumber, user.FullName, user.IsVerified);
    }

    private static string GenerateMockOtp()
    {
        // Mock OTP for development — always returns 123456
        return "123456";
    }
}
