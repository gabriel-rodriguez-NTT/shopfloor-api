namespace ShopfloorAssistant.Core.Email
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(string to, string subject, string body);
    }
}
