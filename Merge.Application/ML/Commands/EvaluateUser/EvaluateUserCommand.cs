using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.EvaluateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluateUserCommand(Guid UserId) : IRequest<FraudAlertDto>;
