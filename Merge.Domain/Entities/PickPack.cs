using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// PickPack Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PickPack : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public string PackNumber { get; set; } = string.Empty; // Auto-generated: PK-XXXXXX
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public PickPackStatus Status { get; set; } = PickPackStatus.Pending;
    public Guid? PickedByUserId { get; set; } // Staff who picked the items
    public Guid? PackedByUserId { get; set; } // Staff who packed the items
    public DateTime? PickedAt { get; set; }
    public DateTime? PackedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? Notes { get; set; }
    public decimal Weight { get; set; } = 0; // Package weight in kg
    public string? Dimensions { get; set; } // Length x Width x Height in cm
    public int PackageCount { get; set; } = 1; // Number of packages

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User? PickedBy { get; set; }
    public User? PackedBy { get; set; }
    public ICollection<PickPackItem> Items { get; set; } = new List<PickPackItem>();
}

