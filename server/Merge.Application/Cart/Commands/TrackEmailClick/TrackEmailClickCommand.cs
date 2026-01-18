using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.TrackEmailClick;

public record TrackEmailClickCommand(Guid EmailId) : IRequest<bool>;

