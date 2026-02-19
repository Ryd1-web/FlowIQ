using FlowIQ.Domain.Entities;

namespace FlowIQ.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
}
