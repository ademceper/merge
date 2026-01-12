using Merge.Domain.SharedKernel;
namespace Merge.Domain.Entities;

// ✅ BOLUM 3.0: Outbox pattern (dual-write sorunu çözümü)
public class OutboxMessage : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

