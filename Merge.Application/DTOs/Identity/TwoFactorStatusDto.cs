using Merge.Domain.Entities;
using Merge.Domain.Enums;
namespace Merge.Application.DTOs.Identity;

public class TwoFactorStatusDto
{
    public bool IsEnabled { get; set; }
    public TwoFactorMethod Method { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int BackupCodesRemaining { get; set; }
}
