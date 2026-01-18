using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// PolicyAcceptance Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class PolicyAcceptance : BaseEntity, IAggregateRoot
{
    public Guid PolicyId { get; private set; }
    public Guid UserId { get; private set; }
    public string AcceptedVersion { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTime AcceptedAt { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true; // False if user revoked acceptance
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Policy Policy { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private PolicyAcceptance() { }

    public static PolicyAcceptance Create(
        Guid policyId,
        Guid userId,
        string acceptedVersion,
        string ipAddress,
        string userAgent)
    {
        Guard.AgainstDefault(policyId, nameof(policyId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(acceptedVersion, nameof(acceptedVersion));
        Guard.AgainstNullOrEmpty(ipAddress, nameof(ipAddress));
        Guard.AgainstNullOrEmpty(userAgent, nameof(userAgent));
        // Configuration değerleri: MaxVersionLength=20, MaxIpAddressLength=45, MaxUserAgentLength=500
        Guard.AgainstLength(acceptedVersion, 20, nameof(acceptedVersion));
        Guard.AgainstLength(ipAddress, 45, nameof(ipAddress));
        Guard.AgainstLength(userAgent, 500, nameof(userAgent));

        var acceptance = new PolicyAcceptance
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            UserId = userId,
            AcceptedVersion = acceptedVersion,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AcceptedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        acceptance.AddDomainEvent(new PolicyAcceptedEvent(acceptance.Id, policyId, userId, acceptedVersion));

        return acceptance;
    }

    public void Revoke()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PolicyAcceptanceRevokedEvent(Id, PolicyId, UserId, AcceptedVersion));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PolicyAcceptanceDeletedEvent(Id, PolicyId, UserId));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PolicyAcceptanceRestoredEvent(Id, PolicyId, UserId));
    }
}
