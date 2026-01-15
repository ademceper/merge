using MediatR;

namespace Merge.Application.Identity.Commands.VerifyBackupCode;

public record VerifyBackupCodeCommand(
    Guid UserId,
    string BackupCode) : IRequest<bool>;

