using Investo.BusinessLogic.Models;

namespace Investo.BusinessLogic.Interfaces;

public interface IUserService
{
    Task<Guid?> RegisterUserAsync(UserCreateModel userCreateModel);

    Task<UserTokenDataModel?> LoginAsync(UserLoginModel userLoginModel);

    Task<UserTokenDataModel?> RefreshTokenAsync(UserTokenDataModel userTokenDataModel);

    Task<UserModel?> GetUserByIdAsync(Guid id);

    Task<bool> UpdateProfileAsync(Guid userId, UserUpdateModel user);

    Task<string?> GetPasswordResetCodeAsync(UserResetPasswordCodeModel resetPassword);

    Task<PasswordResetTokenModel?> VerifyPasswordResetCodeAsync(PasswordResetCodeModel code);

    Task<bool> ResetPasswordAsync(string email, string _password);
}
