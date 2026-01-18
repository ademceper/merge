using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AskQuestion;

public record AskQuestionCommand(
    Guid UserId,
    Guid ProductId,
    string Question
) : IRequest<ProductQuestionDto>;
