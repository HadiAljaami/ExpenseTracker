using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken)
    {
        var resetUrl = $"{_config["App:FrontendUrl"]}/reset-password?token={resetToken}&email={toEmail}";

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #2E86AB;">Password Reset Request</h2>
                <p>Hello <strong>{fullName}</strong>,</p>
                <p>We received a request to reset your password. Click the button below to reset it:</p>
                <a href="{resetUrl}" 
                   style="background-color: #2E86AB; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 4px; display: inline-block; margin: 16px 0;">
                    Reset Password
                </a>
                <p>This link will expire in <strong>1 hour</strong>.</p>
                <p>If you didn't request this, please ignore this email.</p>
                <hr/>
                <small style="color: #888;">Smart Expense Tracker</small>
            </div>
            """;

        await SendEmailAsync(toEmail, "Password Reset Request", body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string fullName)
    {
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #2E86AB;">Welcome to Smart Expense Tracker! 💰</h2>
                <p>Hello <strong>{fullName}</strong>,</p>
                <p>Your account has been created successfully. Start tracking your expenses today!</p>
                <hr/>
                <small style="color: #888;">Smart Expense Tracker</small>
            </div>
            """;

        await SendEmailAsync(toEmail, "Welcome to Smart Expense Tracker!", body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];
        var fromEmail = _config["Email:FromEmail"] ?? smtpUser;
        var fromName = _config["Email:FromName"] ?? "Smart Expense Tracker";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPass)
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail!, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);
        await client.SendMailAsync(message);
    }
}
