using MediatR;

namespace Merge.Application.Product.Commands.UnmarkAnswerHelpful;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UnmarkAnswerHelpfulCommand(
    Guid UserId,
    Guid AnswerId
) : IRequest;
