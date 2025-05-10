using System.ComponentModel.DataAnnotations;

namespace Investo.Api.ViewModels;

public class UserResetPasswordViewModel
{
    [Required]
    [RegularExpression(@"(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,32}")]
    public string NewPassword { get; set; } = string.Empty;
}
