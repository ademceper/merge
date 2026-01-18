using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteAnswer;

public record DeleteAnswerCommand(
    Guid AnswerId,
    Guid UserId
) : IRequest<bool>;
