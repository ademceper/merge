using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Identity;

public class BackupCodesResponseDto
{
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}
