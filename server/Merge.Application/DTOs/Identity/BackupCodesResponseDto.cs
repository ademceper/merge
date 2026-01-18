namespace Merge.Application.DTOs.Identity;

public record BackupCodesResponseDto(
    string[] BackupCodes,
    string Message);
