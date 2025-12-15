using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Web.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var email = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
                Timeout = 8000
            };

            var msg = new MailMessage
            {
                From = new MailAddress(email, "E-Commerce Store"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            msg.To.Add(toEmail);

            await client.SendMailAsync(msg);
        }
    }
}
