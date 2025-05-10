using Investo.BusinessLogic.Interfaces;
using Investo.BusinessLogic.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Investo.BusinessLogic.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings emailSettings;

    public EmailService(IOptions<EmailSettings> options)
    {
        this.emailSettings = options.Value;
    }

    public async Task SendResetCodeAsync(string receiver, string code)
    {
        var message = new MailMessage();
        message.From = new MailAddress(this.emailSettings.From, "Investo");
        message.To.Add(new MailAddress(receiver));
        message.Subject = "Reset Password Code - Inevsto";
        message.IsBodyHtml = true;
        message.Body = $"<p>Hello!</p>\r\n\r\n<p>You have requested a password reset for your <strong>Investo</strong> account.</p>\r\n\r\n<p>Your password reset code is:</p>\r\n\r\n<h2 style=\"color: #2d89ef;\">{code}</h2>\r\n\r\n<p>This code is valid for 10 minutes. If you did not make this request, please ignore this email.</p>\r\n\r\n<br />\r\n\r\n<p style=\"font-size: 0.9em; color: #777;\">Best regards,<br />\r\nThe Investo Team<br />\r\nnoreply.investo@gmail.com</p>\r\n";

        using (var smtpClient = new SmtpClient(this.emailSettings.Host, this.emailSettings.Port))
        {
            smtpClient.EnableSsl = this.emailSettings.EnableSsl;
            smtpClient.Credentials = new System.Net.NetworkCredential(this.emailSettings.UserName, this.emailSettings.Password);
            await smtpClient.SendMailAsync(message);
        }
    }
}
