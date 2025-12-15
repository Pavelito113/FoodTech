using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FoodTech.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        var smtpSection = _config.GetSection("Smtp");
        string host = smtpSection["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
        int port = int.Parse(smtpSection["Port"] ?? "587");
        string user = smtpSection["User"] ?? "";
        string pass = smtpSection["Password"] ?? "";
        string from = smtpSection["From"] ?? user;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };

        var mail = new MailMessage(from, toEmail, subject, body)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mail);
    }
}
