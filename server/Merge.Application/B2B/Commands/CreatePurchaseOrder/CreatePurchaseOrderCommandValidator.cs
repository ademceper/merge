using FluentValidation;

namespace Merge.Application.B2B.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.B2BUserId)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Sipariş verisi boş olamaz");

        RuleFor(x => x.Dto.OrganizationId)
            .NotEmpty().WithMessage("Organizasyon ID boş olamaz");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("Sipariş en az bir ürün içermelidir");
    }
}

