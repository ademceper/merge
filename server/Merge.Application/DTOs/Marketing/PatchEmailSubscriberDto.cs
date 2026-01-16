namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Partial update DTO for Email Subscriber (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchEmailSubscriberDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Source { get; init; }
    public List<string>? Tags { get; init; }
    public Dictionary<string, string>? CustomFields { get; init; }
    public bool? IsSubscribed { get; init; }
}
