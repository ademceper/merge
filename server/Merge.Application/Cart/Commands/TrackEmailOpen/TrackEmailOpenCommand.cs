using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.TrackEmailOpen;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record TrackEmailOpenCommand(Guid EmailId) : IRequest<bool>;

