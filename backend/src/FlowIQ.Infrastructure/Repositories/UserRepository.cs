using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Interfaces;
using FlowIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowIQ.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(FlowIQDbContext context) : base(context) { }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        => await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
}
