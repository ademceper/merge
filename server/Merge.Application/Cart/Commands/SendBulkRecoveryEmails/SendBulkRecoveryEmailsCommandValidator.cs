using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendBulkRecoveryEmails;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SendBulkRecoveryEmailsCommandValidator : AbstractValidator<SendBulkRecoveryEmailsCommand>
{
    public SendBulkRecoveryEmailsCommandValidator()
    {
        RuleFor(x => x.MinHours)
            .GreaterThan(0).WithMessage("Minimum saat 0'dan büyük olmalıdır");
    }
}

