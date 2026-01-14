using MediatR;

namespace Merge.Application.Search.Commands.RecordClick;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RecordClickCommand(
    Guid SearchHistoryId,
    Guid ProductId
) : IRequest;
