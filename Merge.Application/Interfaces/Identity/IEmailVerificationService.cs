namespace Merge.Application.Interfaces.Identity;

public interface IEmailVerificationService
{
    Task<string> GenerateVerificationTokenAsync(Guid userId, string email);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(Guid userId);
    Task<bool> IsEmailVerifiedAsync(Guid userId);
}

