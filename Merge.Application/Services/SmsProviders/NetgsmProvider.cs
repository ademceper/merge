using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.SmsProviders;

namespace Merge.Application.Services.SmsProviders;

public class NetgsmProvider : ISmsProvider
{
    public string ProviderName => "Netgsm";
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<NetgsmProvider> _logger;

    public NetgsmProvider(IConfiguration configuration, ILogger<NetgsmProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SmsSendResult> SendSmsAsync(SmsMessage message)
    {
        _logger.LogInformation("Netgsm SMS sending started. To: {To}", message.To);
        
        var username = _configuration["SmsProviders:Netgsm:Username"];
        var password = _configuration["SmsProviders:Netgsm:Password"];
        var senderId = _configuration["SmsProviders:Netgsm:SenderId"] ?? "MERGE";
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("Netgsm API credentials not configured");
            return new SmsSendResult
            {
                Success = false,
                ErrorMessage = "SMS provider not configured"
            };
        }

        // Mock implementation - Gerçek implementasyonda Netgsm API kullanılacak
        // var client = new HttpClient();
        // var response = await client.GetAsync($"https://api.netgsm.com.tr/sms/send/get?usercode={username}&password={password}&gsmno={message.To}&message={message.Message}&msgheader={senderId}");
        
        await Task.Delay(100);
        
        var messageId = $"NETGSM_{Guid.NewGuid():N}";
        
        _logger.LogInformation("Netgsm SMS sent successfully. MessageId: {MessageId}", messageId);
        
        return new SmsSendResult
        {
            Success = true,
            MessageId = messageId,
            Metadata = new Dictionary<string, object>
            {
                { "provider", "Netgsm" },
                { "to", message.To },
                { "senderId", senderId }
            }
        };
    }

    public Task<bool> VerifyConfigurationAsync()
    {
        var username = _configuration["SmsProviders:Netgsm:Username"];
        var password = _configuration["SmsProviders:Netgsm:Password"];
        return Task.FromResult(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password));
    }
}

