using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class ReturnRequest : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty; // Defective, WrongItem, NotAsDescribed, ChangedMind
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public ReturnRequestStatus Status { get; set; } = ReturnRequestStatus.Pending;
    public string? RejectionReason { get; set; }
    public decimal RefundAmount { get; set; }
    public string? TrackingNumber { get; set; } // İade kargo takip no
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<Guid> OrderItemIds { get; set; } = new List<Guid>(); // İade edilecek kalemler

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public User User { get; set; } = null!;
}

