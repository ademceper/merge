using MediatR;

namespace Merge.Application.Search.Commands.RecordClick;

public record RecordClickCommand(
    Guid SearchHistoryId,
    Guid ProductId
) : IRequest;
