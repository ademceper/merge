using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Cart.Commands.SendBulkRecoveryEmails;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
public record SendBulkRecoveryEmailsCommand(
    int MinHours = 2,
    AbandonedCartEmailType EmailType = AbandonedCartEmailType.First
) : IRequest;

