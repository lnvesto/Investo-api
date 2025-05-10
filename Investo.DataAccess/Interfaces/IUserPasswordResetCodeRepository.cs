using Investo.DataAccess.Entities;

namespace Investo.DataAccess.Interfaces;

public interface IUserPasswordResetCodeRepository : ICrudRepository<UserResetPasswordCode, int>
{
    Task<UserResetPasswordCode?> GetByUserIdAsync(Guid userId);

    Task DeleteAllByUserIdAsync(Guid userId);
}
