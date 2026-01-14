using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.RegenerateBackupCodes;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RegenerateBackupCodesCommand(
    Guid UserId,
    RegenerateBackupCodesDto RegenerateDto) : IRequest<BackupCodesResponseDto>;

