namespace Merge.Domain.Enums;

/// <summary>
/// Alert Type Enum - BOLUM 1.2: Enum kullanımı (string AlertType YASAK)
/// </summary>
public enum AlertType
{
    Account = 0,
    Payment = 1,
    Order = 2,
    System = 3,
    Security = 4,
    Fraud = 5,
    Compliance = 6,
    Performance = 7,
    Other = 99
}
