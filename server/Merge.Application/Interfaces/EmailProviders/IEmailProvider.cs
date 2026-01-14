namespace Merge.Application.Interfaces.EmailProviders;

public interface IEmailProvider
{
    string ProviderName { get; }
    Task<EmailSendResult> SendEmailAsync(EmailMessage message);
    Task<bool> VerifyConfigurationAsync();
}

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? From { get; set; }
    public string? FromName { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

