using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ApproveAnswer;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApproveAnswerCommand(
    Guid AnswerId
) : IRequest<bool>;
