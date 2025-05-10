namespace Investo.BusinessLogic.Models;

public class UserResetPasswordCodeModel
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string ResetCode { get; set; } = string.Empty;

    public DateTime ExpirationDate { get; set; }
}
