using MediatR;

namespace Merge.Application.Identity.Commands.VerifyBackupCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record VerifyBackupCodeCommand(
    Guid UserId,
    string BackupCode) : IRequest<bool>;

