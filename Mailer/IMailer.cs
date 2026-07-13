public interface IMailer
{
    public Task SendEmailAsync(string subject, string body);
}