using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Ticket Category - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum TicketCategory
{
    Order,
    Product,
    Payment,
    Shipping,
    Return,
    Account,
    Technical,
    Other
}

