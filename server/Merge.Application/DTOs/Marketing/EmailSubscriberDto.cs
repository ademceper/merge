using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;


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
