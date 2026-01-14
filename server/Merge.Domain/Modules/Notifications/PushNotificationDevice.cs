using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// PushNotificationDevice Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PushNotificationDevice : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    private string _deviceToken = string.Empty;
    public string DeviceToken 
    { 
        get => _deviceToken; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(DeviceToken));
            Guard.AgainstLength(value, 500, nameof(DeviceToken));
            _deviceToken = value;
        }
    }
    
    private string _platform = string.Empty;
    public string Platform 
    { 
        get => _platform; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Platform));
            Guard.AgainstLength(value, 50, nameof(Platform));
            _platform = value;
        }
    }
    
    private string? _deviceId;
    public string? DeviceId 
    { 
        get => _deviceId; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 200, nameof(DeviceId));
            }
            _deviceId = value;
        }
    }
    
    private string? _deviceModel;
    public string? DeviceModel 
    { 
        get => _deviceModel; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 100, nameof(DeviceModel));
            }
            _deviceModel = value;
        }
    }
    
    private string? _appVersion;
    public string? AppVersion 
    { 
        get => _appVersion; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 50, nameof(AppVersion));
            }
            _appVersion = value;
        }
    }
    
    public bool IsActive { get; private set; } = true;
    public DateTime? LastUsedAt { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PushNotificationDevice() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PushNotificationDevice Create(
        Guid userId,
        string deviceToken,
        string platform,
        string? deviceId = null,
        string? deviceModel = null,
        string? appVersion = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(deviceToken, nameof(deviceToken));
        Guard.AgainstLength(deviceToken, 500, nameof(deviceToken));
        Guard.AgainstNullOrEmpty(platform, nameof(platform));
        Guard.AgainstLength(platform, 50, nameof(platform));
        if (deviceId != null)
        {
            Guard.AgainstLength(deviceId, 200, nameof(deviceId));
        }
        if (deviceModel != null)
        {
            Guard.AgainstLength(deviceModel, 100, nameof(deviceModel));
        }
        if (appVersion != null)
        {
            Guard.AgainstLength(appVersion, 50, nameof(appVersion));
        }

        return new PushNotificationDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _deviceToken = deviceToken,
            _platform = platform,
            _deviceId = deviceId,
            _deviceModel = deviceModel,
            _appVersion = appVersion,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update device token
    public void UpdateDeviceToken(string deviceToken)
    {
        Guard.AgainstNullOrEmpty(deviceToken, nameof(deviceToken));
        Guard.AgainstLength(deviceToken, 500, nameof(deviceToken));

        _deviceToken = deviceToken;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update device info
    public void UpdateDeviceInfo(string? deviceModel = null, string? appVersion = null)
    {
        if (deviceModel != null)
        {
            Guard.AgainstLength(deviceModel, 100, nameof(deviceModel));
            _deviceModel = deviceModel;
        }
        if (appVersion != null)
        {
            Guard.AgainstLength(appVersion, 50, nameof(appVersion));
            _appVersion = appVersion;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Activate device
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate device
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Record last used
    public void RecordLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Delete device (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Cihaz zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

