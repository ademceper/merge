using MediatR;

namespace Merge.Application.Search.Commands.RecordSearch;

public record RecordSearchCommand(
    string SearchTerm,
    Guid? UserId,
    int ResultCount,
    string? UserAgent = null,
    string? IpAddress = null
) : IRequest;
