using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Commands.CreateSecurityAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateSecurityAlertCommand(
    Guid? UserId,
    string AlertType,
    string Severity,
    string Title,
    string Description,
    SecurityEventMetadataDto? Metadata = null
) : IRequest<SecurityAlertDto>;
