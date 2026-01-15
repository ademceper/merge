using MediatR;

namespace Merge.Application.LiveCommerce.Commands.ResumeStream;

public record ResumeStreamCommand(Guid StreamId) : IRequest<Unit>;
