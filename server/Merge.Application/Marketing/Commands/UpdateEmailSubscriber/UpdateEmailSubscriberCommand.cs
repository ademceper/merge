using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.UpdateEmailSubscriber;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateEmailSubscriberCommand(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Source,
    List<string>? Tags,
    Dictionary<string, string>? CustomFields,
    bool? IsSubscribed) : IRequest<EmailSubscriberDto>;
