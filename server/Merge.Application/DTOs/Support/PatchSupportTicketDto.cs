namespace Merge.Application.DTOs.Support;

/// <summary>
/// Partial update DTO for Support Ticket (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchSupportTicketDto
{
    public string? Subject { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string? Priority { get; init; }
    public string? Status { get; init; }
    public Guid? AssignedToId { get; init; }
}
