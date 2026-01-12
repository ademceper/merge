using FluentValidation;
using Merge.Application.Order.Commands.CreateReturnRequest;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CreateReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Validation
public class CreateReturnRequestCommandValidator : AbstractValidator<CreateReturnRequestCommand>
{
    public CreateReturnRequestCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Return request bilgileri boş olamaz.");

        RuleFor(x => x.Dto!.OrderId)
            .NotEmpty().WithMessage("Sipariş ID boş olamaz.");

        RuleFor(x => x.Dto!.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.Dto!.Reason)
            .NotEmpty().WithMessage("İade nedeni boş olamaz.")
            .MinimumLength(5).WithMessage("İade nedeni en az 5 karakter olmalıdır.")
            .MaximumLength(1000).WithMessage("İade nedeni en fazla 1000 karakter olmalıdır.");

        RuleFor(x => x.Dto!.OrderItemIds)
            .NotEmpty().WithMessage("En az bir ürün seçilmelidir.");
    }
}
