namespace Portfolio.Models.Settings;

public class MailSettings
{
    public string DisplayName { get; set; } = default!;
    public string Host { get; set; } = default!;
    public string Mail { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int Port { get; set; } = default!;
}