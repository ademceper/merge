namespace Merge.Domain.Entities;

/// <summary>
/// PopularSearch Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PopularSearch : BaseEntity
{
    public string SearchTerm { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public int ClickThroughCount { get; set; }
    public decimal ClickThroughRate { get; set; }
    public DateTime LastSearchedAt { get; set; }
}

