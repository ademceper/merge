using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.CreateProductComparison;

public class CreateProductComparisonCommandValidator : AbstractValidator<CreateProductComparisonCommand>
{
    public CreateProductComparisonCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("En az bir ürün seçilmelidir.")
            .Must(p => p.Count >= 2).WithMessage("En az 2 ürün karşılaştırılmalıdır.")
            .Must(p => p.Count <= 5).WithMessage("Aynı anda en fazla 5 ürün karşılaştırılabilir.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("İsim en fazla 200 karakter olabilir.");
    }
}
