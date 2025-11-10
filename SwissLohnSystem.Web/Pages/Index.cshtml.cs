using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SwissLohnSystem.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostSendMessage()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
                emailMessage.To.Add(MailboxAddress.Parse(emailSettings["SenderEmail"]));
                emailMessage.Subject = $"Neue Nachricht von {Name}";
                emailMessage.Body = new TextPart("plain")
                {
                    Text = $"Name: {Name}\nEmail: {Email}\nNachricht:\n{Message}"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                // Baþarýlý mesajý TempData’ya kaydet
                TempData["SuccessMessage"] = "Vielen Dank! Ihre Nachricht wurde gesendet.";

                // Redirect ile PRG uygulanýyor
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fehler beim Senden: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
