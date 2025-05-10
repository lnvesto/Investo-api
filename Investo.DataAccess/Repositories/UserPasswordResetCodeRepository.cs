using Investo.DataAccess.EF;
using Investo.DataAccess.Entities;
using Investo.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investo.DataAccess.Repositories;

public class UserPasswordResetCodeRepository : AbstractRepository<UserResetPasswordCode, int>, IUserPasswordResetCodeRepository
{
    public UserPasswordResetCodeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserResetPasswordCode?> GetByUserIdAsync(Guid userId)
    {
        return await this.dbSet.OrderBy(o => o).LastOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task DeleteAllByUserIdAsync(Guid userId)
    {
        var resetCodes = await this.dbSet.Where(x => x.UserId == userId).ToListAsync();
        if (resetCodes.Count > 0)
        {
            this.dbSet.RemoveRange(resetCodes);
            await this.context.SaveChangesAsync();
        }
    }
}
