using Merge.Domain.Modules.Marketing;
namespace Merge.Domain.Enums;

/// <summary>
/// Live stream status values for LiveStream entity
/// </summary>
public enum LiveStreamStatus
{
    Scheduled = 0,
    Live = 1,
    Paused = 2,
    Ended = 3,
    Cancelled = 4
}
