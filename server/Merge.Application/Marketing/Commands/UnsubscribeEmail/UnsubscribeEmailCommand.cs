using MediatR;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Commands.UnsubscribeEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UnsubscribeEmailCommand(string Email) : IRequest<bool>;
