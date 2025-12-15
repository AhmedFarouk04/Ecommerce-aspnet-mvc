namespace ECommerce.Application.Interfaces
{
    public interface IEmailSenderService
    {
        Task<bool> SendAsync(string to, string subject, string htmlBody);
        Task<bool> SendResetPasswordAsync(string email, string token);
    }
}
