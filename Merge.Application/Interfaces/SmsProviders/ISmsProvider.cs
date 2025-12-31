namespace Merge.Application.Interfaces.SmsProviders;

public interface ISmsProvider
{
    string ProviderName { get; }
    Task<SmsSendResult> SendSmsAsync(SmsMessage message);
    Task<bool> VerifyConfigurationAsync();
}

public class SmsMessage
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SenderId { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SmsSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

