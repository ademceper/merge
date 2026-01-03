namespace Merge.Domain.Enums;

/// <summary>
/// Stock Movement Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum StockMovementType
{
    Receipt,        // Receiving stock (purchase)
    Sale,           // Stock sold to customer
    Return,         // Customer return
    Transfer,       // Transfer between warehouses
    Adjustment,     // Manual adjustment (inventory count correction)
    Damage,         // Damaged goods
    Lost,           // Lost/stolen goods
    Reserved,       // Reserved for order
    Released        // Released from reservation
}

