using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.RegenerateBackupCodes;

public record RegenerateBackupCodesCommand(
    Guid UserId,
    RegenerateBackupCodesDto RegenerateDto) : IRequest<BackupCodesResponseDto>;

