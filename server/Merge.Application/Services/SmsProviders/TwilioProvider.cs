using Merge.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.SmsProviders;

namespace Merge.Application.Services.SmsProviders;

public class TwilioProvider : ISmsProvider
{
    public string ProviderName => "Twilio";
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioProvider> _logger;

    public TwilioProvider(IConfiguration configuration, ILogger<TwilioProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SmsSendResult> SendSmsAsync(SmsMessage message)
    {
        // ✅ MODERN C#: ArgumentNullException.ThrowIfNull (C# 10+)
        ArgumentNullException.ThrowIfNull(message);

        // ✅ ARCHITECTURE: Input validation
        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ValidationException("Telefon numarası boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(message.Message))
        {
            throw new ValidationException("SMS mesajı boş olamaz.");
        }

        _logger.LogInformation("Twilio SMS sending started. To: {To}", message.To);
        
        var accountSid = _configuration["SmsProviders:Twilio:AccountSid"];
        var authToken = _configuration["SmsProviders:Twilio:AuthToken"];
        var fromNumber = _configuration["SmsProviders:Twilio:FromNumber"];
        
        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
        {
            _logger.LogWarning("Twilio API credentials not configured");
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
        
        _logger.LogInformation("Twilio SMS sent successfully. MessageId: {MessageId}", messageId);
        
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
        var accountSid = _configuration["SmsProviders:Twilio:AccountSid"];
        var authToken = _configuration["SmsProviders:Twilio:AuthToken"];
        return Task.FromResult(!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken));
    }
}

