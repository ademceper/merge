// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Identity;

// ⚠️ OBSOLETE: Bu interface artık kullanılmamalı. MediatR Command/Query pattern kullanılmalı.
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service'ler yerine Command/Query handler'ları kullan
[Obsolete("Use MediatR Commands/Queries instead. This interface will be removed in a future version.")]
public interface IEmailVerificationService
{
    Task<string> GenerateVerificationTokenAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ResendVerificationEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailVerifiedAsync(Guid userId, CancellationToken cancellationToken = default);
}

