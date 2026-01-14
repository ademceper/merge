using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
namespace Merge.Domain.Enums;

/// <summary>
/// ReportType Enum - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Enum'lar ayrı dosyalarda olmalı (Merge.Domain/Enums klasöründe)
/// </summary>
public enum ReportType
{
    Sales,
    Revenue,
    Products,
    Inventory,
    Customers,
    Orders,
    Marketing,
    Financial,
    Tax,
    Shipping,
    Returns,
    Custom
}

