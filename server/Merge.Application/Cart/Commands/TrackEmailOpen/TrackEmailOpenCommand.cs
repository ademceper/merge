using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.TrackEmailOpen;

public record TrackEmailOpenCommand(Guid EmailId) : IRequest<bool>;

