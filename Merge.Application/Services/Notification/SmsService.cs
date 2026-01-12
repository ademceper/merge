using Microsoft.Extensions.Configuration;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Application.Services.Notification;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Services.Notification;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendOrderConfirmationSmsAsync(string phoneNumber, string orderNumber);
    Task SendOtpAsync(string phoneNumber, string otp);
}

public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Burada gerçek SMS servisi entegrasyonu yapılacak (Twilio, Netgsm, vb.)
        // Şimdilik sadece loglama yapıyoruz
        _logger.LogInformation("SMS gönderiliyor: To={PhoneNumber}, Message={Message}", phoneNumber, message);
        
        // Gerçek implementasyon için:
        // - Twilio, Netgsm, veya diğer SMS provider'ları kullanılabilir
        // - Queue'ya eklenip background job ile gönderilebilir
        
        await Task.CompletedTask;
    }

    public async Task SendOrderConfirmationSmsAsync(string phoneNumber, string orderNumber)
    {
        var message = $"Siparişiniz alındı! Sipariş No: {orderNumber}. Merge E-Ticaret";
        await SendSmsAsync(phoneNumber, message);
    }

    public async Task SendOtpAsync(string phoneNumber, string otp)
    {
        var message = $"Doğrulama kodunuz: {otp}. Merge E-Ticaret";
        await SendSmsAsync(phoneNumber, message);
    }
}

