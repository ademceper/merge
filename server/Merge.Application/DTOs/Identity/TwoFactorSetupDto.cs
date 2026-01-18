using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Identity;

public record TwoFactorSetupDto(
    TwoFactorMethod Method,
    string? PhoneNumber,
    string? Email);
