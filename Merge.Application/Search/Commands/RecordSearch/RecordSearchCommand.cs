using MediatR;

namespace Merge.Application.Search.Commands.RecordSearch;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RecordSearchCommand(
    string SearchTerm,
    Guid? UserId,
    int ResultCount,
    string? UserAgent = null,
    string? IpAddress = null
) : IRequest;
