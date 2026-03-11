using EmailWorkerService.Messages;

namespace EmailWorkerService.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailNotificationMessage message);
    }
}