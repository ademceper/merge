namespace Merge.Domain.Enums;

/// <summary>
/// Ticket Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum TicketStatus
{
    Open,
    InProgress,
    Waiting,
    Resolved,
    Closed
}
