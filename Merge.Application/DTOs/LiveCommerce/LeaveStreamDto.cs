using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record LeaveStreamDto(
    [StringLength(100)] string? GuestId);
