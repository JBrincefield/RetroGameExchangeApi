using EmailWorkerService.Messages;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailWorkerService.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailNotificationMessage message)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    "Retro Game Exchange",
                    _configuration["Smtp:FromAddress"]));
                email.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
                email.Subject = message.Subject;

                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = message.Body
                };

                using var smtp = new SmtpClient();
                
                var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
                var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var username = _configuration["Smtp:Username"];
                var password = _configuration["Smtp:Password"];

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", message.ToEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", message.ToEmail);
                throw;
            }
        }
    }
}