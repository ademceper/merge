using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.EvaluateOrder;

public record EvaluateOrderCommand(Guid OrderId) : IRequest<FraudAlertDto>;
