namespace LMS.Infrastructure.Settings;

/// <summary>Resend configuration — bound from the "Resend" appsettings section.</summary>
public class ResendSettings
{
    public string ApiKey    { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
}
