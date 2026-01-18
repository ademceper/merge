using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Identity;

public record TwoFactorStatusDto(
    bool IsEnabled,
    TwoFactorMethod Method,
    string? PhoneNumber,
    string? Email,
    int BackupCodesRemaining);
