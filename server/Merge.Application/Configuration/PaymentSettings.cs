using Merge.Domain.Modules.Payment;
namespace Merge.Application.Configuration;

/// <summary>
/// Payment işlemleri için configuration ayarları
/// </summary>
public class PaymentSettings
{
    public const string SectionName = "PaymentSettings";

    /// <summary>
    /// Ödeme işlemi için maksimum retry sayısı
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Fatura vadesi (gün)
    /// </summary>
    public int InvoiceDueDays { get; set; } = 30;
}

