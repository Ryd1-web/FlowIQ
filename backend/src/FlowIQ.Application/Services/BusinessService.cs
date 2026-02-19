using FlowIQ.Application.DTOs.Business;
using FlowIQ.Application.Exceptions;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;

namespace FlowIQ.Application.Services;

public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<BusinessResponse> CreateAsync(Guid userId, CreateBusinessRequest request)
    {
        var business = new Business
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Address = request.Address,
            UserId = userId
        };

        await _businessRepository.AddAsync(business);
        return MapToResponse(business);
    }

    public async Task<BusinessResponse> UpdateAsync(Guid businessId, Guid userId, UpdateBusinessRequest request)
    {
        var business = await _businessRepository.GetByIdAsync(businessId)
            ?? throw new NotFoundException("Business", businessId);

        if (business.UserId != userId)
            throw new UnauthorizedException();

        business.Name = request.Name;
        business.Description = request.Description;
        business.Category = request.Category;
        business.Address = request.Address;
        business.UpdatedAt = DateTime.UtcNow;

        await _businessRepository.UpdateAsync(business);
        return MapToResponse(business);
    }

    public async Task<BusinessResponse?> GetByIdAsync(Guid businessId, Guid userId)
    {
        var business = await _businessRepository.GetByIdAsync(businessId)
            ?? throw new NotFoundException("Business", businessId);

        if (business.UserId != userId)
            throw new UnauthorizedException();

        return MapToResponse(business);
    }

    public async Task<IEnumerable<BusinessResponse>> GetByUserIdAsync(Guid userId)
    {
        var businesses = await _businessRepository.GetByUserIdAsync(userId);
        return businesses.Select(MapToResponse);
    }

    private static BusinessResponse MapToResponse(Business b)
        => new(b.Id, b.Name, b.Description, b.Category, b.Address, b.UserId, b.CreatedAt);
}
