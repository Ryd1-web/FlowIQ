using FlowIQ.Application.DTOs.Business;

namespace FlowIQ.Application.Interfaces;

public interface IBusinessService
{
    Task<BusinessResponse> CreateAsync(Guid userId, CreateBusinessRequest request);
    Task<BusinessResponse> UpdateAsync(Guid businessId, Guid userId, UpdateBusinessRequest request);
    Task<BusinessResponse?> GetByIdAsync(Guid businessId, Guid userId);
    Task<IEnumerable<BusinessResponse>> GetByUserIdAsync(Guid userId);
}
