using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailVerification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailVerification : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private EmailVerification() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static EmailVerification Create(
        Guid userId,
        string email,
        string token,
        DateTime expiresAt)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(email, nameof(email));
        Guard.AgainstNullOrEmpty(token, nameof(token));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Verification token expiration date must be in the future");

        var verification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            Token = token,
            ExpiresAt = expiresAt,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        return verification;
    }

    // ✅ BOLUM 1.1: Domain Method - Verify email
    public void Verify()
    {
        if (IsVerified)
            throw new DomainException("Email is already verified");

        if (IsExpired)
            throw new DomainException("Verification token has expired");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailVerifiedEvent
        AddDomainEvent(new EmailVerifiedEvent(UserId, Email, Id));
    }

    // ✅ BOLUM 1.1: Domain Logic - Computed property
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

