using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.EvaluateUser;

public record EvaluateUserCommand(Guid UserId) : IRequest<FraudAlertDto>;
