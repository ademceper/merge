namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email Subscriber DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EmailSubscriberDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsSubscribed,
    DateTime SubscribedAt,
    DateTime? UnsubscribedAt,
    string? Source,
    int EmailsSent,
    int EmailsOpened,
    int EmailsClicked,
    List<string> Tags);
