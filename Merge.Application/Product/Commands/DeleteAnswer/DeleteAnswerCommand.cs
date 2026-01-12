using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteAnswer;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteAnswerCommand(
    Guid AnswerId,
    Guid UserId
) : IRequest<bool>;
