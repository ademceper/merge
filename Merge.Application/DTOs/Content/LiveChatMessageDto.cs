namespace Merge.Application.DTOs.Content;

public class LiveChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? SenderId { get; set; }
    public string? SenderName { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
