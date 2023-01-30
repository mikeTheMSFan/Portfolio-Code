#region Imports

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Portfolio.Models.Settings;
using Portfolio.Services.Interfaces;

#endregion

namespace Portfolio.Services;

public class MWSEmailService : IMWSEmailService
{
    private readonly AppSettings _appSettings;

    public MWSEmailService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    #region Send contect Email

    public async Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
    {
        //Write email
        var email = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_appSettings.MailSettings.Mail),
            To = { MailboxAddress.Parse("bigmike2238@gmx.com") },
            From = { MailboxAddress.Parse("noreply@mikemrobinsondev.com") },
            Subject = subject
        };

        //define email body using passed in HTML string.
        var builder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };

        //use builder to define email body.
        email.Body = builder.ToMessageBody();

        //Send email
        var smtp = new SmtpClient();
        await smtp.ConnectAsync(_appSettings.MailSettings.Host, _appSettings.MailSettings.Port,
            SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_appSettings.MailSettings.Mail, _appSettings.MailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    #endregion

    #region Send email

    public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
    {
        //Write email
        var email = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_appSettings.MailSettings.Mail),
            To = { MailboxAddress.Parse(emailTo) },
            From = { MailboxAddress.Parse("noreply@mikemrobinsondev.com") },
            Subject = subject
        };

        //define email body using passed in HTML string.
        var builder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };

        //use builder to define email body.
        email.Body = builder.ToMessageBody();

        //Send email
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_appSettings.MailSettings.Host, _appSettings.MailSettings.Port,
            SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_appSettings.MailSettings.Mail, _appSettings.MailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    #endregion
}