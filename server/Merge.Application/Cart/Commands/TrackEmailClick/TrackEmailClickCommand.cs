using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.TrackEmailClick;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record TrackEmailClickCommand(Guid EmailId) : IRequest<bool>;

