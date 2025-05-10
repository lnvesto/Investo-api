using System.ComponentModel.DataAnnotations;

namespace Investo.Api.ViewModels;

public class PasswordResetCodeViewModel
{
    [Required]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must consist of 6 digits")]
    public string ResetCode { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
}
