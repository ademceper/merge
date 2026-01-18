using Merge.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.SmsProviders;

namespace Merge.Application.Services.SmsProviders;

public class TwilioProvider(IConfiguration configuration, ILogger<TwilioProvider> logger) : ISmsProvider
{
    public string ProviderName => "Twilio";

    public async Task<SmsSendResult> SendSmsAsync(SmsMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ValidationException("Telefon numarası boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(message.Message))
        {
            throw new ValidationException("SMS mesajı boş olamaz.");
        }

        logger.LogInformation("Twilio SMS sending started. To: {To}", message.To);
        
        var accountSid = configuration["SmsProviders:Twilio:AccountSid"];
        var authToken = configuration["SmsProviders:Twilio:AuthToken"];
        var fromNumber = configuration["SmsProviders:Twilio:FromNumber"];
        
        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
        {
            logger.LogWarning("Twilio API credentials not configured");
            return new SmsSendResult
            {
                Success = false,
                ErrorMessage = "SMS provider not configured"
            };
        }

        // Mock implementation - Gerçek implementasyonda Twilio SDK kullanılacak
        // TwilioClient.Init(accountSid, authToken);
        // var twilioMessage = await MessageResource.CreateAsync(...);
        
        await Task.Delay(100);
        
        var messageId = $"TW_{Guid.NewGuid():N}";
        
        logger.LogInformation("Twilio SMS sent successfully. MessageId: {MessageId}", messageId);
        
        return new SmsSendResult
        {
            Success = true,
            MessageId = messageId,
            Metadata = new Dictionary<string, object>
            {
                ["provider"] = "Twilio",
                ["to"] = message.To
            }
        };
    }

    public Task<bool> VerifyConfigurationAsync()
    {
        var accountSid = configuration["SmsProviders:Twilio:AccountSid"];
        var authToken = configuration["SmsProviders:Twilio:AuthToken"];
        return Task.FromResult(!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken));
    }
}

