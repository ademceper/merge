namespace Merge.Domain.Enums;

/// <summary>
/// Tax Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// BOLUM 1.2: Enum kullanımı (string TaxType YASAK)
/// </summary>
public enum TaxType
{
    VAT = 0,           // Value Added Tax
    GST = 1,           // Goods and Services Tax
    SalesTax = 2,      // Sales Tax
    ServiceTax = 3,    // Service Tax
    ExciseTax = 4,     // Excise Tax
    CustomDuty = 5,    // Custom Duty
    Other = 99         // Other tax types
}
