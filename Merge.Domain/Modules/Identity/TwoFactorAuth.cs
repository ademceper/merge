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
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
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

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Remove domain event
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private TwoFactorAuth() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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
        
        // ✅ BOLUM 1.3: Value Objects - Email validation
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

        // ✅ BOLUM 1.5: Domain Events - TwoFactorAuthCreatedEvent
        twoFactorAuth.AddDomainEvent(new TwoFactorAuthCreatedEvent(twoFactorAuth.Id, userId, method));

        return twoFactorAuth;
    }

    // ✅ BOLUM 1.1: Domain Method - Enable 2FA
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

        // ✅ BOLUM 1.5: Domain Events - TwoFactorEnabledEvent
        AddDomainEvent(new TwoFactorEnabledEvent(Id, UserId, Method));
    }

    // ✅ BOLUM 1.1: Domain Method - Disable 2FA
    public void Disable()
    {
        if (!IsEnabled)
            throw new DomainException("2FA is not enabled");

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TwoFactorDisabledEvent
        AddDomainEvent(new TwoFactorDisabledEvent(UserId, Id, Method));
    }

    // ✅ BOLUM 1.1: Domain Method - Update 2FA setup (for reconfiguration)
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
        
        // ✅ BOLUM 1.3: Value Objects - Email validation
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

    // ✅ BOLUM 1.1: Domain Method - Verify 2FA setup
    public void Verify()
    {
        if (IsVerified)
            throw new DomainException("2FA is already verified");

        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new TwoFactorVerifiedEvent(Id, UserId, Method));
    }

    // ✅ BOLUM 1.1: Domain Method - Record failed attempt
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

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new TwoFactorFailedAttemptRecordedEvent(Id, UserId, Method, FailedAttempts, IsLocked && !wasLocked));
    }

    // ✅ BOLUM 1.1: Domain Method - Reset failed attempts
    public void ResetFailedAttempts()
    {
        if (FailedAttempts == 0 && !IsLocked) return;
        
        FailedAttempts = 0;
        LastAttemptAt = DateTime.UtcNow;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new TwoFactorFailedAttemptsResetEvent(Id, UserId, Method));
    }

    // ✅ BOLUM 1.1: Domain Method - Update backup codes
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

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new TwoFactorBackupCodesUpdatedEvent(Id, UserId, Method, backupCodes.Length));
    }

    // ✅ BOLUM 1.1: Domain Method - Remove backup code
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

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new TwoFactorBackupCodeRemovedEvent(Id, UserId, Method, remainingCodes.Length));
    }

    // ✅ BOLUM 1.1: Domain Logic - Computed properties
    public bool IsLocked => LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value;
    public bool CanAttempt => !IsLocked;
}

