using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// TwoFactorAuth Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string Status YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TwoFactorAuth : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    
    public TwoFactorMethod Method { get; private set; }
    public string Secret { get; private set; } = string.Empty; // For authenticator apps (TOTP secret)
    public string? PhoneNumber { get; private set; } // For SMS
    public string? Email { get; private set; } // For email
    public bool IsEnabled { get; private set; } = false;
    public bool IsVerified { get; private set; } = false;
    public string[]? BackupCodes { get; private set; } // Backup codes for recovery
    public int FailedAttempts { get; private set; } = 0;
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    private TwoFactorAuth() { }

    public static TwoFactorAuth Create(
        Guid userId,
        TwoFactorMethod method,
        string secret,
        string? phoneNumber = null,
        string? email = null,
        string[]? backupCodes = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(secret, nameof(secret));
        Guard.AgainstLength(secret, 100, nameof(secret));

        if (method == TwoFactorMethod.SMS && string.IsNullOrEmpty(phoneNumber))
            throw new DomainException("Phone number is required for SMS 2FA method");
        
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var cleanedPhone = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (cleanedPhone.Length < 10 || cleanedPhone.Length > 15)
                throw new DomainException("Geçersiz telefon numarası formatı");
        }

        if (method == TwoFactorMethod.Email && string.IsNullOrEmpty(email))
            throw new DomainException("Email is required for Email 2FA method");
        
        if (!string.IsNullOrEmpty(email))
        {
            var emailValueObject = new Email(email);
            email = emailValueObject.Value;
        }
        
        // Validate backup codes if provided
        if (backupCodes != null && backupCodes.Length > 0)
        {
            foreach (var code in backupCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                    throw new DomainException("Backup code cannot be null or empty");
                
                Guard.AgainstLength(code, 20, nameof(backupCodes));
            }
        }

        var twoFactorAuth = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Method = method,
            Secret = secret,
            PhoneNumber = phoneNumber,
            Email = email,
            BackupCodes = backupCodes,
            IsEnabled = false,
            IsVerified = false,
            FailedAttempts = 0,
            CreatedAt = DateTime.UtcNow
        };

        twoFactorAuth.AddDomainEvent(new TwoFactorAuthCreatedEvent(twoFactorAuth.Id, userId, method));

        return twoFactorAuth;
    }

    public void Enable()
    {
        if (IsEnabled)
            throw new DomainException("2FA is already enabled");

        if (!IsVerified)
            throw new DomainException("2FA must be verified before enabling");

        if (IsLocked)
            throw new DomainException("2FA account is locked");

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorEnabledEvent(Id, UserId, Method));
    }

    public void Disable()
    {
        if (!IsEnabled)
            throw new DomainException("2FA is not enabled");

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorDisabledEvent(UserId, Id, Method));
    }

    public void UpdateSetup(
        TwoFactorMethod method,
        string secret,
        string? phoneNumber = null,
        string? email = null,
        string[]? backupCodes = null)
    {
        Guard.AgainstNullOrEmpty(secret, nameof(secret));
        Guard.AgainstLength(secret, 100, nameof(secret));

        if (method == TwoFactorMethod.SMS && string.IsNullOrEmpty(phoneNumber))
            throw new DomainException("Phone number is required for SMS 2FA method");
        
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var cleanedPhone = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (cleanedPhone.Length < 10 || cleanedPhone.Length > 15)
                throw new DomainException("Geçersiz telefon numarası formatı");
        }

        if (method == TwoFactorMethod.Email && string.IsNullOrEmpty(email))
            throw new DomainException("Email is required for Email 2FA method");
        
        if (!string.IsNullOrEmpty(email))
        {
            var emailValueObject = new Email(email);
            email = emailValueObject.Value;
        }
        
        // Validate backup codes if provided
        if (backupCodes != null && backupCodes.Length > 0)
        {
            foreach (var code in backupCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                    throw new DomainException("Backup code cannot be null or empty");
                
                Guard.AgainstLength(code, 20, nameof(backupCodes));
            }
        }

        Method = method;
        Secret = secret;
        PhoneNumber = phoneNumber;
        Email = email;
        if (backupCodes != null)
        {
            BackupCodes = backupCodes;
        }
        IsEnabled = false;
        IsVerified = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Verify()
    {
        if (IsVerified)
            throw new DomainException("2FA is already verified");

        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorVerifiedEvent(Id, UserId, Method));
    }

    // NOT: Lockout logic configuration'dan gelmeli, ancak entity'de business rule olarak tutuluyor
    // Configuration değerleri handler'dan geçirilmeli veya entity'ye inject edilmeli
    // Şimdilik parametre olarak alıyoruz
    public void RecordFailedAttempt(int maxFailedAttempts = 5, int lockoutMinutes = 15)
    {
        FailedAttempts++;
        LastAttemptAt = DateTime.UtcNow;

        // Lock account after max failed attempts
        bool wasLocked = IsLocked;
        if (FailedAttempts >= maxFailedAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorFailedAttemptRecordedEvent(Id, UserId, Method, FailedAttempts, IsLocked && !wasLocked));
    }

    public void ResetFailedAttempts()
    {
        if (FailedAttempts == 0 && !IsLocked) return;
        
        FailedAttempts = 0;
        LastAttemptAt = DateTime.UtcNow;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorFailedAttemptsResetEvent(Id, UserId, Method));
    }

    public void UpdateBackupCodes(string[] backupCodes)
    {
        Guard.AgainstNull(backupCodes, nameof(backupCodes));
        
        if (backupCodes.Length == 0)
            throw new DomainException("Backup codes cannot be empty");
        
        // Validate each backup code length
        foreach (var code in backupCodes)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Backup code cannot be null or empty");
            
            Guard.AgainstLength(code, 20, nameof(backupCodes));
        }

        BackupCodes = backupCodes;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorBackupCodesUpdatedEvent(Id, UserId, Method, backupCodes.Length));
    }

    public void RemoveBackupCode(string backupCode)
    {
        Guard.AgainstNullOrEmpty(backupCode, nameof(backupCode));
        Guard.AgainstLength(backupCode, 20, nameof(backupCode));

        if (BackupCodes == null || BackupCodes.Length == 0)
            throw new DomainException("No backup codes available");

        // Normalize backup code for comparison
        var normalizedCode = backupCode.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant();

        // Find and remove matching code
        var remainingCodes = BackupCodes
            .Where(c => c.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant() != normalizedCode)
            .ToArray();

        if (remainingCodes.Length == BackupCodes.Length)
            throw new DomainException("Backup code not found");

        BackupCodes = remainingCodes;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorBackupCodeRemovedEvent(Id, UserId, Method, remainingCodes.Length));
    }

    public bool IsLocked => LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value;
    public bool CanAttempt => !IsLocked;
}

