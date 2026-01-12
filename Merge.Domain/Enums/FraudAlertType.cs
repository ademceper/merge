using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Fraud Alert Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum FraudAlertType
{
    Order,
    Payment,
    Account,
    Behavior
}
