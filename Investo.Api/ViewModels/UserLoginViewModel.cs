namespace Investo.Api.ViewModels;

using System.ComponentModel.DataAnnotations;

public class UserLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,32}")]
    public string Password { get; set; } = string.Empty;
}