using MediatR;

namespace Merge.Application.Product.Commands.MarkAnswerHelpful;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkAnswerHelpfulCommand(
    Guid UserId,
    Guid AnswerId
) : IRequest;
