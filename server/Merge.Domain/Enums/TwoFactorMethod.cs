using Merge.Domain.ValueObjects;
namespace Merge.Domain.Enums;

/// <summary>
/// Two Factor Method - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum TwoFactorMethod
{
    None,
    SMS,
    Email,
    Authenticator
}

