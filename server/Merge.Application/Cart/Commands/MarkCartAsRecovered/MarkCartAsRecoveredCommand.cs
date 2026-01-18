using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.MarkCartAsRecovered;

public record MarkCartAsRecoveredCommand(Guid CartId) : IRequest;

