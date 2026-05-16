namespace LMS.Infrastructure.Interfaces;

/// <summary>Sends transactional emails via Resend.</summary>
public interface IEmailService
{
    /// <summary>Sends a welcome email to a new user.</summary>
    Task SendWelcomeEmailAsync(string toEmail, string userName, string tempPasswordResetLink);

    /// <summary>Sends a password reset email.</summary>
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
}
