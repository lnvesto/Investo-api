namespace Investo.BusinessLogic.Services;

using AutoMapper;
using Investo.BusinessLogic.Interfaces;
using Investo.BusinessLogic.Models;
using Investo.DataAccess.EF;
using Investo.DataAccess.Entities;
using Investo.DataAccess.Interfaces;
using Investo.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly IJwtService jwtService;
    private readonly IUserRepository userRepository;
    private readonly IUserPasswordResetCodeRepository userPasswordResetCodeRepository;
    private readonly IMapper mapper;
    private readonly IConfiguration configuration;

    public UserService(ApplicationDbContext context, IJwtService jwtService, IMapper mapper, IConfiguration configuration)
    {
        this.userRepository = new UserRepository(context);
        this.userPasswordResetCodeRepository = new UserPasswordResetCodeRepository(context);
        this.jwtService = jwtService;
        this.mapper = mapper;
        this.configuration = configuration;
    }

    public async Task<UserTokenDataModel?> LoginAsync(UserLoginModel userLoginModel)
    {
        var passwordSalt = await this.userRepository.GetUserSalt(userLoginModel.Email);
        if (passwordSalt is null)
        {
            return null;
        }

        byte[] password = Encoding.UTF8.GetBytes(userLoginModel.Password);
        string hashedPassword = GenerateHash(password, Convert.FromBase64String(passwordSalt));

        var userEntity = await this.userRepository.LoginAsync(userLoginModel.Email, hashedPassword);
        if (userEntity is null)
        {
            return null;
        }

        string token = this.jwtService.GenerateToken(this.mapper.Map<UserModel>(userEntity));

        string? refreshToken = await this.SetNewRefreshToken(userEntity.Id);
        if (refreshToken is null)
        {
            return null;
        }

        await this.userPasswordResetCodeRepository.DeleteAllByUserIdAsync(userEntity.Id);

        return new UserTokenDataModel
        {
            Token = token,
            RefreshToken = refreshToken,
        };
    }

    public async Task<Guid?> RegisterUserAsync(UserCreateModel userCreateModel)
    {
        var existingEntity = await this.userRepository.GetByEmailAsync(userCreateModel.Email);
        if (existingEntity is not null)
        {
            return null;
        }

        byte[] salt = RandomNumberGenerator.GetBytes(32);
        byte[] password = Encoding.UTF8.GetBytes(userCreateModel.Password);

        string hashPassword = GenerateHash(password, salt);
        string saltString = Convert.ToBase64String(salt);

        User? userEntity = this.mapper.Map<User>(userCreateModel);

        userEntity.PasswordHash = hashPassword;
        userEntity.PasswordSalt = saltString;
        userEntity.UserTypeId = userCreateModel.UserTypeId;
        userEntity.RegistrationDate = DateTime.UtcNow;

        return await this.userRepository.CreateAsync(userEntity);
    }

    public async Task<UserTokenDataModel?> RefreshTokenAsync(UserTokenDataModel userTokenDataModel)
    {
        ClaimsPrincipal? principals = this.jwtService.GetPrincipalFromExpiredToken(userTokenDataModel.Token);
        if (principals is null)
        {
            return null;
        }

        var id = principals.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id is null)
        {
            return null;
        }

        Guid userId = Guid.Parse(id);
        string? oldRefreshToken = await this.userRepository.GetRefreshToken(userId);
        if (oldRefreshToken is null || !oldRefreshToken.Equals(userTokenDataModel.RefreshToken))
        {
            return null;
        }

        string? newRefreshToken = await this.SetNewRefreshToken(userId);
        if (newRefreshToken is null)
        {
            return null;
        }

        var userEnity = await this.userRepository.GetByIdAsync(userId);
        string? newToken = this.jwtService.GenerateToken(this.mapper.Map<UserModel>(userEnity));

        return new UserTokenDataModel
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
        };
    }

    public async Task<UserModel?> GetUserByIdAsync(Guid id)
    {
        var userEntity = await this.userRepository.GetByIdAsync(id);
        if (userEntity is null)
        {
            return null;
        }

        return this.mapper.Map<UserModel>(userEntity);
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UserUpdateModel user)
    {
        user.Id = userId;
        var userEntity = await userRepository.GetByIdAsync(userId);
        if (userEntity is null)
        {
            return false;
        }

        userEntity.Email = user.Email;
        userEntity.FirstName = user.FirstName;
        userEntity.LastName = user.LastName;

        await this.userRepository.UpdateAsync(userEntity);

        return true;
    }

    public async Task<string?> GetPasswordResetCodeAsync(UserResetPasswordCodeModel resetPassword)
    {
        var userEntity = await this.userRepository.GetByEmailAsync(resetPassword.Email);
        if (userEntity is null)
        {
            return null;
        }

        resetPassword.UserId = userEntity.Id;
        resetPassword.ResetCode = GenerateResetCode();
        resetPassword.ExpirationDate = DateTime.UtcNow.AddMinutes(int.Parse(this.configuration["ResetCode:ExpirationMinutes"] ?? "10"));

        await this.userPasswordResetCodeRepository.CreateAsync(this.mapper.Map<UserResetPasswordCode>(resetPassword));

        return resetPassword.ResetCode;
    }

    public async Task<PasswordResetTokenModel?> VerifyPasswordResetCodeAsync(PasswordResetCodeModel code)
    {
        var userEntity = await this.userRepository.GetByEmailAsync(code.Email);
        if (userEntity is null)
        {
            return null;
        }

        var resetCodeEntity = await this.userPasswordResetCodeRepository.GetByUserIdAsync(userEntity.Id);
        if (resetCodeEntity is null || resetCodeEntity.ResetCode != code.ResetCode)
        {
            return null;
        }

        if (resetCodeEntity.ExpirationDate < DateTime.UtcNow)
        {
            return null;
        }

        string temporaryToken = this.jwtService.GenerateToken(code);
        await this.userPasswordResetCodeRepository.DeleteAllByUserIdAsync(userEntity.Id);
        return new PasswordResetTokenModel
        {
            TemporaryToken = temporaryToken,
        };
    }

    public async Task<bool> ResetPasswordAsync(string email, string _password)
    {
        var userEntity = await this.userRepository.GetByEmailAsync(email);
        if (userEntity is null)
        {
            return false;
        }
        byte[] salt = RandomNumberGenerator.GetBytes(32);
        byte[] password = Encoding.UTF8.GetBytes(_password);

        string hashPassword = GenerateHash(password, salt);
        string saltString = Convert.ToBase64String(salt);

        userEntity.PasswordHash = hashPassword;
        userEntity.PasswordSalt = saltString;

        await this.userRepository.UpdateAsync(userEntity);

        return true;
    }

    private async Task<string?> SetNewRefreshToken(Guid userId)
    {
        string refreshToken = this.jwtService.GenerateRefreshToken();
        DateTime expireDate = DateTime.UtcNow.AddDays(int.Parse(this.configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));

        return (await this.userRepository.SetRefreshToken(userId, refreshToken, expireDate) is not null) ? refreshToken : null;
    }

    private static string GenerateHash(byte[] password, byte[] salt)
    {
        byte[] hash = SHA256.HashData(password.Concat(salt).ToArray());
        return Convert.ToBase64String(hash);
    }

    private static string GenerateResetCode()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(6);
        int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
        return (value % 1_000_000).ToString("D6");
    }
}
