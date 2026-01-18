using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendBulkRecoveryEmails;

public record SendBulkRecoveryEmailsCommand(
    int MinHours = 2,
    AbandonedCartEmailType EmailType = AbandonedCartEmailType.First
) : IRequest;

