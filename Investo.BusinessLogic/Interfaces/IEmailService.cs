namespace Investo.BusinessLogic.Interfaces;

public interface IEmailService
{
    Task SendResetCodeAsync(string receiver, string code);
}
