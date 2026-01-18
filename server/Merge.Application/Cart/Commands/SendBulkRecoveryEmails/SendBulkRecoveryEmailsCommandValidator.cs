using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendBulkRecoveryEmails;

public class SendBulkRecoveryEmailsCommandValidator : AbstractValidator<SendBulkRecoveryEmailsCommand>
{
    public SendBulkRecoveryEmailsCommandValidator()
    {
        RuleFor(x => x.MinHours)
            .GreaterThan(0).WithMessage("Minimum saat 0'dan büyük olmalıdır");
    }
}

