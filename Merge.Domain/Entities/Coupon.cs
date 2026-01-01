using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; } // Sabit indirim tutarı
    public decimal? DiscountPercentage { get; set; } // Yüzde indirim
    public decimal? MinimumPurchaseAmount { get; set; } // Minimum alışveriş tutarı
    public decimal? MaximumDiscountAmount { get; set; } // Maksimum indirim tutarı
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UsageLimit { get; set; } = 0; // 0 = sınırsız
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsForNewUsersOnly { get; set; } = false;
    public List<Guid>? ApplicableCategoryIds { get; set; } // Belirli kategoriler için
    public List<Guid>? ApplicableProductIds { get; set; } // Belirli ürünler için

    // ✅ CONCURRENCY: RowVersion for optimistic concurrency control (kupon kullanımı için kritik)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

