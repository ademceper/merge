namespace Merge.Domain.Enums;

/// <summary>
/// Business Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum BusinessType
{
    Individual = 0,
    Company = 1,
    Partnership = 2,
    LLC = 3,
    Corporation = 4,
    SoleProprietorship = 5,
    Other = 6
}
