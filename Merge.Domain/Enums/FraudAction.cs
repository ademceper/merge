using Merge.Domain.Modules.Catalog;
namespace Merge.Domain.Enums;

/// <summary>
/// Fraud Detection Action - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum FraudAction
{
    Flag,
    Block,
    Review,
    Alert
}
