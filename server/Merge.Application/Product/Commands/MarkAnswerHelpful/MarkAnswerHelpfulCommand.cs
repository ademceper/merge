using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.MarkAnswerHelpful;

public record MarkAnswerHelpfulCommand(
    Guid UserId,
    Guid AnswerId
) : IRequest;
