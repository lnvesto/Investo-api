namespace Investo.BusinessLogic.Services;

using Investo.BusinessLogic.Interfaces;
using Investo.BusinessLogic.Models;
using Investo.DataAccess.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtService : IJwtService
{
    private readonly IConfiguration configuration;

    public JwtService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerateToken(UserModel userModel)
    {
        var claim = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userModel.Id.ToString()),
            new Claim(ClaimTypes.Role, Enum.GetName((UserTypes)userModel.UserTypeId)!),
            new Claim("firstName", userModel.FirstName),
            new Claim("lastName", userModel.LastName),
            new Claim("email", userModel.Email),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claim,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(this.configuration["Jwt:AccessTokenExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(PasswordResetCodeModel codeModel)
    {
        var claim = new[]
        {
            new Claim(ClaimTypes.Email, codeModel.Email),
            new Claim("scope", "password_reset"),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claim,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(this.configuration["ResetCode:ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = this.configuration["Jwt:Issuer"],
            ValidAudience = this.configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"]!)
            ),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(
            SecurityAlgorithms.HmacSha256,
            StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }
}
