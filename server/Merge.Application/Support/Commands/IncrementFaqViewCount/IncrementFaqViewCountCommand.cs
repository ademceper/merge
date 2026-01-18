using MediatR;

namespace Merge.Application.Support.Commands.IncrementFaqViewCount;

public record IncrementFaqViewCountCommand(
    Guid FaqId
) : IRequest<bool>;
