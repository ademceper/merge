using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Identity;

public record VerifyBackupCodeDto(
    [Required] Guid UserId,
    [Required] [StringLength(9, MinimumLength = 9, ErrorMessage = "Yedek kod 9 karakter olmalıdır (XXXX-XXXX formatında).")] string BackupCode);

