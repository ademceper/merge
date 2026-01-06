namespace Merge.Domain.Entities;

/// <summary>
/// LiveStreamOrder Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamOrder : BaseEntity
{
    public Guid LiveStreamId { get; set; }
    public LiveStream LiveStream { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? ProductId { get; set; } // Product that triggered the order
    public Product? Product { get; set; }
    // CreatedAt is inherited from BaseEntity
    public decimal OrderAmount { get; set; }
}

