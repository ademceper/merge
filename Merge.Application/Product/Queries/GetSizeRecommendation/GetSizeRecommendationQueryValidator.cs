using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeRecommendation;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetSizeRecommendationQueryValidator : AbstractValidator<GetSizeRecommendationQuery>
{
    public GetSizeRecommendationQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Boy 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(300).WithMessage("Boy en fazla 300 cm olabilir.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Kilo 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(300).WithMessage("Kilo en fazla 300 kg olabilir.");

        RuleFor(x => x.Chest)
            .GreaterThan(0).When(x => x.Chest.HasValue)
            .WithMessage("Göğüs ölçüsü 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(200).When(x => x.Chest.HasValue)
            .WithMessage("Göğüs ölçüsü en fazla 200 cm olabilir.");

        RuleFor(x => x.Waist)
            .GreaterThan(0).When(x => x.Waist.HasValue)
            .WithMessage("Bel ölçüsü 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(200).When(x => x.Waist.HasValue)
            .WithMessage("Bel ölçüsü en fazla 200 cm olabilir.");
    }
}
