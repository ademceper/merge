// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Identity;

public interface IEmailVerificationService
{
    Task<string> GenerateVerificationTokenAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ResendVerificationEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailVerifiedAsync(Guid userId, CancellationToken cancellationToken = default);
}

