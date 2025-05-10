namespace Investo.BusinessLogic.Models;

public class PasswordResetCodeModel
{
    public string ResetCode { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
