using Investo.DataAccess.Entities;

namespace Investo.DataAccess.Interfaces;

public interface IUserRepository : ICrudRepository<User, Guid>
{
    Task<string?> GetUserSalt(string email);

    Task<User?> LoginAsync(string email, string passwordHash);

    Task<string?> SetRefreshToken(Guid userId, string refreshToken, DateTime expireDate);

    Task<string?> GetRefreshToken(Guid userId);

    Task<User?> GetByEmailAsync(string email);
}
