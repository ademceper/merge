using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// B2B User Settings DTO - BOLUM 4.3: Over-Posting Korumasi (ZORUNLU)
/// Dictionary<string, object> yerine typed DTO kullanılıyor
/// </summary>
public class B2BUserSettingsDto
{
    [StringLength(100)]
    public string? Theme { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    public bool? EnableNotifications { get; set; }

    public bool? EnableEmailAlerts { get; set; }

    [StringLength(500)]
    public string? CustomPreferences { get; set; }
}

