using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Payment Check Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum PaymentCheckType
{
    CVV = 0,
    ThreeDS = 1,
    Address = 2,
    Velocity = 3,
    Device = 4
}
