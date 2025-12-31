using Microsoft.Extensions.Configuration;
using NotificationEntity = Merge.Domain.Entities.Notification;
using Merge.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Merge.Application.Services.Notification;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount);
    Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber);
    Task SendPasswordResetAsync(string to, string resetToken);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        // Burada gerçek email servisi entegrasyonu yapılacak (SendGrid, SMTP, vb.)
        // Şimdilik sadece loglama yapıyoruz
        _logger.LogInformation("Email gönderiliyor: To={To}, Subject={Subject}", to, subject);
        
        // Gerçek implementasyon için:
        // - SendGrid, MailKit, veya SMTP kullanılabilir
        // - Email template'leri kullanılabilir
        // - Queue'ya eklenip background job ile gönderilebilir
        
        await Task.CompletedTask;
    }

    public async Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount)
    {
        var subject = $"Sipariş Onayı - {orderNumber}";
        var body = $@"
            <h2>Siparişiniz Alındı!</h2>
            <p>Sipariş Numaranız: <strong>{orderNumber}</strong></p>
            <p>Toplam Tutar: <strong>{totalAmount:C}</strong></p>
            <p>Siparişiniz en kısa sürede hazırlanacaktır.</p>
        ";
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber)
    {
        var subject = $"Siparişiniz Kargoya Verildi - {orderNumber}";
        var body = $@"
            <h2>Siparişiniz Kargoya Verildi!</h2>
            <p>Sipariş Numaranız: <strong>{orderNumber}</strong></p>
            <p>Takip Numarası: <strong>{trackingNumber}</strong></p>
            <p>Siparişinizi takip edebilirsiniz.</p>
        ";
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendPasswordResetAsync(string to, string resetToken)
    {
        var subject = "Şifre Sıfırlama";
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={resetToken}";
        var body = $@"
            <h2>Şifre Sıfırlama</h2>
            <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
            <p><a href=""{resetUrl}"">Şifre Sıfırla</a></p>
            <p>Bu link 1 saat geçerlidir.</p>
        ";
        await SendEmailAsync(to, subject, body);
    }
}

