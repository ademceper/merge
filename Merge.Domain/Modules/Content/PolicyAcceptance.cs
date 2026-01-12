using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
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
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid PolicyId { get; private set; }
    public Guid UserId { get; private set; }
    public string AcceptedVersion { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTime AcceptedAt { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true; // False if user revoked acceptance
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Policy Policy { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PolicyAcceptance() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.5: Domain Events - PolicyAcceptedEvent yayınla
        acceptance.AddDomainEvent(new PolicyAcceptedEvent(acceptance.Id, policyId, userId, acceptedVersion));

        return acceptance;
    }

    // ✅ BOLUM 1.1: Domain Method - Revoke acceptance
    public void Revoke()
    {
        if (!IsActive)
        {
            throw new DomainException("Policy acceptance zaten iptal edilmiş.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyAcceptanceRevokedEvent yayınla
        AddDomainEvent(new PolicyAcceptanceRevokedEvent(Id, PolicyId, UserId, AcceptedVersion));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
