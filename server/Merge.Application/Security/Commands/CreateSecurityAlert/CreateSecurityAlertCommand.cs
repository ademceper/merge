using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Commands.CreateSecurityAlert;

public record CreateSecurityAlertCommand(
    Guid? UserId,
    string AlertType,
    string Severity,
    string Title,
    string Description,
    SecurityEventMetadataDto? Metadata = null
) : IRequest<SecurityAlertDto>;
