using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.EmailProviders;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Services.EmailProviders;

public class SendGridProvider : IEmailProvider
{
    public string ProviderName => "SendGrid";
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridProvider> _logger;

    public SendGridProvider(IConfiguration configuration, ILogger<SendGridProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendEmailAsync(EmailMessage message)
    {
        _logger.LogInformation("SendGrid email sending started. To: {To}, Subject: {Subject}", message.To, message.Subject);
        
        var apiKey = _configuration["EmailProviders:SendGrid:ApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured");
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "Email provider not configured"
            };
        }

        // Mock implementation - Gerçek implementasyonda SendGrid SDK kullanılacak
        // var client = new SendGridClient(apiKey);
        // var msg = MailHelper.CreateSingleEmail(...);
        // var response = await client.SendEmailAsync(msg);
        
        await Task.Delay(100);
        
        var messageId = $"SG_{Guid.NewGuid():N}";
        
        _logger.LogInformation("SendGrid email sent successfully. MessageId: {MessageId}", messageId);
        
        return new EmailSendResult
        {
            Success = true,
            MessageId = messageId,
            Metadata = new Dictionary<string, object>
            {
                ["provider"] = "SendGrid",
                ["to"] = message.To
            }
        };
    }

    public Task<bool> VerifyConfigurationAsync()
    {
        var apiKey = _configuration["EmailProviders:SendGrid:ApiKey"];
        return Task.FromResult(!string.IsNullOrEmpty(apiKey));
    }
}

