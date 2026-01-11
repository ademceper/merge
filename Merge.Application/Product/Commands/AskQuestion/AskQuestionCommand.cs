using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.AskQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AskQuestionCommand(
    Guid UserId,
    Guid ProductId,
    string Question
) : IRequest<ProductQuestionDto>;
