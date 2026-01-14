using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Commands.SubscribeEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SubscribeEmailCommand(
    string Email,
    string? FirstName,
    string? LastName,
    string? Source,
    List<string>? Tags,
    Dictionary<string, string>? CustomFields) : IRequest<EmailSubscriberDto>;
