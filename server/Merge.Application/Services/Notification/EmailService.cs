using Microsoft.Extensions.Configuration;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Application.Services.Notification;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Services.Notification;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount, CancellationToken cancellationToken = default);
    Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string to, string resetToken, CancellationToken cancellationToken = default);
}

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        // Burada gerçek email servisi entegrasyonu yapılacak (SendGrid, SMTP, vb.)
        // Şimdilik sadece loglama yapıyoruz
        logger.LogInformation("Email gönderiliyor: To={To}, Subject={Subject}", to, subject);
        
        // Gerçek implementasyon için:
        // - SendGrid, MailKit, veya SMTP kullanılabilir
        // - Email template'leri kullanılabilir
        // - Queue'ya eklenip background job ile gönderilebilir
        
        await Task.CompletedTask;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        var subject = $"Sipariş Onayı - {orderNumber}";
        var body = $@"
            <h2>Siparişiniz Alındı!</h2>
            <p>Sipariş Numaranız: <strong>{orderNumber}</strong></p>
            <p>Toplam Tutar: <strong>{totalAmount:C}</strong></p>
            <p>Siparişiniz en kısa sürede hazırlanacaktır.</p>
        ";
        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default)
    {
        var subject = $"Siparişiniz Kargoya Verildi - {orderNumber}";
        var body = $@"
            <h2>Siparişiniz Kargoya Verildi!</h2>
            <p>Sipariş Numaranız: <strong>{orderNumber}</strong></p>
            <p>Takip Numarası: <strong>{trackingNumber}</strong></p>
            <p>Siparişinizi takip edebilirsiniz.</p>
        ";
        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SendPasswordResetAsync(string to, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Şifre Sıfırlama";
        var resetUrl = $"{configuration["App:BaseUrl"]}/reset-password?token={resetToken}";
        var body = $@"
            <h2>Şifre Sıfırlama</h2>
            <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
            <p><a href=""{resetUrl}"">Şifre Sıfırla</a></p>
            <p>Bu link 1 saat geçerlidir.</p>
        ";
        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}

