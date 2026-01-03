namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Customer Lifetime Value DTO - BOLUM 4.2: Sensitive Data Exposure (YASAK)
/// Typed DTO kullanılıyor (object yerine)
/// </summary>
public class CustomerLifetimeValueDto
{
    public Guid CustomerId { get; set; }
    public decimal LifetimeValue { get; set; }
}

