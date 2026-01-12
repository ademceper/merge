using Merge.Domain.Modules.Support;
namespace Merge.Domain.Enums;

/// <summary>
/// Communication status for CustomerCommunication entity
/// </summary>
public enum CommunicationStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Read = 3,
    Failed = 4,
    Bounced = 5
}
