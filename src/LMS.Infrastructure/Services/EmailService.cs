using System.Net.Http.Json;
using LMS.Infrastructure.Interfaces;
using LMS.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ResendSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<ResendSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, string tempPasswordResetLink)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto;">
              <div style="background: #1e293b; padding: 24px; border-radius: 8px 8px 0 0;">
                <h1 style="color: white; margin: 0; font-size: 24px;">Welcome to LMS</h1>
              </div>
              <div style="background: white; padding: 24px; border: 1px solid #e2e8f0;">
                <p style="color: #334155;">Hi {userName},</p>
                <p style="color: #334155;">
                  Your account has been created.
                  Click the button below to set your password.
                </p>
                <a href="{tempPasswordResetLink}"
                   style="display: inline-block; background: #2563eb;
                          color: white; padding: 12px 24px;
                          border-radius: 6px; text-decoration: none;
                          font-weight: 600; margin: 16px 0;">
                  Set Your Password
                </a>
                <p style="color: #94a3b8; font-size: 14px;">This link expires in 24 hours.</p>
              </div>
              <div style="background: #f8fafc; padding: 16px;
                          border-radius: 0 0 8px 8px; text-align: center;">
                <p style="color: #94a3b8; font-size: 12px; margin: 0;">
                  Villa College — UWE Bristol LMS
                </p>
              </div>
            </div>
            """;

        await SendAsync(toEmail, "Welcome to LMS — Set your password", html);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto;">
              <div style="background: #1e293b; padding: 24px; border-radius: 8px 8px 0 0;">
                <h1 style="color: white; margin: 0; font-size: 24px;">Reset Your Password</h1>
              </div>
              <div style="background: white; padding: 24px; border: 1px solid #e2e8f0;">
                <p style="color: #334155;">Hi {userName},</p>
                <p style="color: #334155;">
                  We received a request to reset your password.
                  Click the button below to continue.
                </p>
                <a href="{resetLink}"
                   style="display: inline-block; background: #2563eb;
                          color: white; padding: 12px 24px;
                          border-radius: 6px; text-decoration: none;
                          font-weight: 600; margin: 16px 0;">
                  Reset Password
                </a>
                <p style="color: #94a3b8; font-size: 14px;">
                  This link expires in 30 minutes.
                  If you did not request this, ignore this email.
                </p>
              </div>
              <div style="background: #f8fafc; padding: 16px;
                          border-radius: 0 0 8px 8px; text-align: center;">
                <p style="color: #94a3b8; font-size: 12px; margin: 0;">
                  Villa College — UWE Bristol LMS
                </p>
              </div>
            </div>
            """;

        await SendAsync(toEmail, "LMS — Password Reset Request", html);
    }

    private async Task SendAsync(string toEmail, string subject, string html)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

            var payload = new
            {
                from = _settings.FromEmail,
                to = new[] { toEmail },
                subject,
                html
            };

            var response = await client.PostAsJsonAsync("https://api.resend.com/emails", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend email failed: {StatusCode} {Error}", response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            // Never throw — email failure must never break the main request flow
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
