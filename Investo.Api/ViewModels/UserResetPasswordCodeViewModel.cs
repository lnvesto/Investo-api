using System.ComponentModel.DataAnnotations;

namespace Investo.Api.ViewModels;

public class UserResetPasswordCodeViewModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;
}
