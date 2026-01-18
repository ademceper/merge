using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.UpdateEmailSubscriber;

public record UpdateEmailSubscriberCommand(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Source,
    List<string>? Tags,
    Dictionary<string, string>? CustomFields,
    bool? IsSubscribed) : IRequest<EmailSubscriberDto>;
