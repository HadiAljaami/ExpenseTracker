namespace Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken);
    Task SendWelcomeEmailAsync(string toEmail, string fullName);
}
