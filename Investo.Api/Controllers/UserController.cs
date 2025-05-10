using AutoMapper;
using Investo.Api.ViewModels;
using Investo.BusinessLogic.Interfaces;
using Investo.BusinessLogic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Investo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly IEmailService emailService;
    private readonly IMapper mapper;

    public UserController(IUserService userService, IMapper mapper, IEmailService emailService)
    {
        this.userService = userService;
        this.mapper = mapper;
        this.emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserTokenDataModel>> CreateUser([FromBody] UserCreateViewModel user)
    {
        var entry = await this.userService.RegisterUserAsync(this.mapper.Map<UserCreateModel>(user));
        if (entry is null)
        {
            return this.BadRequest(new { message = "User with this email already exists" });
        }

        UserTokenDataModel? userToken = await this.userService.LoginAsync(this.mapper.Map<UserLoginModel>(user));
        if (userToken is null)
        {
            return this.BadRequest(new { message = "Invalid email or password" });
        }

        return this.Ok(userToken);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserTokenDataModel>> LoginUser([FromBody] UserLoginViewModel user)
    {
        UserTokenDataModel? userToken = await this.userService.LoginAsync(this.mapper.Map<UserLoginModel>(user));
        if (userToken is null)
        {
            return this.BadRequest(new { message = "Invalid email or password" });
        }

        return this.Ok(userToken);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<UserTokenDataModel>> RefreshToken([FromBody] UserRefreshTokenViewModel oldToken)
    {
        UserTokenDataModel? userToken = await this.userService.RefreshTokenAsync(new UserTokenDataModel
        {
            Token = oldToken.OldToken,
            RefreshToken = oldToken.RefreshToken,
        });
        if (userToken is null)
        {
            return this.BadRequest(new { message = "Invalid refresh token" });
        }

        return this.Ok(userToken);
    }

    [HttpGet("profile/{id:guid}")]
    public async Task<ActionResult<UserModel>> GetProfile([FromRoute] Guid id)
    {
        var user = await this.userService.GetUserByIdAsync(id);
        if (user is null)
        {
            return this.NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult> UpdateProfile([FromBody] UserUpdateViewModel userUpdateModel)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        bool result = await this.userService.UpdateProfileAsync(userId, this.mapper.Map<UserUpdateModel>(userUpdateModel));
        if (!result)
        {
            return this.BadRequest(new { message = "Failed to update user profile" });
        }

        return Ok();
    }

    [HttpPost("reset-password/request")]
    public async Task<ActionResult> RequestPasswordReset([FromBody] UserResetPasswordCodeViewModel user)
    {
        var resetCode = await this.userService.GetPasswordResetCodeAsync(this.mapper.Map<UserResetPasswordCodeModel>(user));
        if (resetCode is null)
        {
            return this.BadRequest(new { message = "Invalid email" });
        }

        await this.emailService.SendResetCodeAsync(user.Email, resetCode);

        return this.Ok();
    }

    [HttpPost("reset-password/verify")]
    public async Task<ActionResult<PasswordResetTokenModel>> VerifyPasswordReset([FromBody] PasswordResetCodeViewModel code)
    {
        var temporaryToken = await this.userService.VerifyPasswordResetCodeAsync(this.mapper.Map<PasswordResetCodeModel>(code));
        if (temporaryToken is null)
        {
            return this.BadRequest(new { message = "Invalid or expired reset code." });
        }

        return this.Ok(temporaryToken);
    }

    [Authorize]
    [HttpPost("reset-password/confirm")]
    public async Task<ActionResult> ConfirmPasswordReset([FromBody] UserResetPasswordViewModel user)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var scope = User.FindFirst("scope")?.Value;

        if (scope != "password_reset" || string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        bool success = await this.userService.ResetPasswordAsync(email, user.NewPassword);
        if (!success)
        {
            return this.BadRequest(new { message = "Failed to reset password" });
        }

        return this.Ok();
    }
}
