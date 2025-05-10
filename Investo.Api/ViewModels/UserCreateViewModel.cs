namespace Investo.Api.ViewModels;

using System.ComponentModel.DataAnnotations;

public class UserCreateViewModel
{
    [Required]
    [RegularExpression(@"^\w{2,100}$")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\w{2,100}$")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,32}")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int UserTypeId { get; set; }
}