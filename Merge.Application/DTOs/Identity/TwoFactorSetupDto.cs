using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Identity;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record TwoFactorSetupDto(
    TwoFactorMethod Method,
    string? PhoneNumber,
    string? Email);
