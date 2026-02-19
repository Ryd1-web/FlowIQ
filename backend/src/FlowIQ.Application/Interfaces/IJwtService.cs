using FlowIQ.Domain.Entities;

namespace FlowIQ.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
