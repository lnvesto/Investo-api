using System.ComponentModel.DataAnnotations.Schema;

namespace Investo.DataAccess.Entities;

[Table("user_reset_password_codes")]
public class UserResetPasswordCode: AbstractEntity<int>
{
    [Column("reset_code")]
    public string ResetCode { get; set; } = string.Empty;

    [Column("user_id")]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Column("expiration_date")]
    public DateTime ExpirationDate { get; set; }

    public User? User { get; set; }
}
