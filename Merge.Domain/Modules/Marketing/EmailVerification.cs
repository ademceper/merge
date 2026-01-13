using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// EmailVerification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailVerification : BaseEntity, IAggregateRoot
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

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Schema.Timestamp]
    public byte[]? RowVersion { get; set; }

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

        // ✅ BOLUM 1.5: Domain Events - EmailVerificationCreatedEvent
        verification.AddDomainEvent(new EmailVerificationCreatedEvent(verification.Id, userId, email));

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

