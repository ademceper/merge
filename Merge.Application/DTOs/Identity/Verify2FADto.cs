using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Identity;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record Verify2FADto(
    [Required] Guid UserId,
    [Required] [StringLength(10, MinimumLength = 4, ErrorMessage = "Kod en az 4, en fazla 10 karakter olmalıdır.")] string Code);
