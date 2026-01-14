using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Identity;
namespace Merge.Domain.Enums;

/// <summary>
/// Generic entity status for Organization, Store, B2BUser entities
/// </summary>
public enum EntityStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3
}
