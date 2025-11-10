using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string name, string email, string message)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
        emailMessage.To.Add(MailboxAddress.Parse(emailSettings["SenderEmail"]));
        emailMessage.Subject = $"Neue Nachricht von {name}";
        emailMessage.Body = new TextPart("plain")
        {
            Text = $"Name: {name}\nEmail: {email}\nNachricht:\n{message}"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}
