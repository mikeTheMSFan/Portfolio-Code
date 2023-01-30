namespace Portfolio.Services.Interfaces;

public interface IMWSEmailService
{
    public Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage);
    public Task SendEmailAsync(string email, string subject, string htmlMessage);
}