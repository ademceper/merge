using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.ExportOrders;

public class ExportOrdersCommandValidator : AbstractValidator<ExportOrdersCommand>
{
    public ExportOrdersCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Başlangıç tarihi bitiş tarihinden büyük olamaz.");

        RuleFor(x => x.Format)
            .IsInEnum()
            .WithMessage("Geçersiz export formatı.");
    }
}
