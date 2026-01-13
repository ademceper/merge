using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetGiftCardByCode;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetGiftCardByCodeQueryValidator() : AbstractValidator<GetGiftCardByCodeQuery>
{
    public GetGiftCardByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Hediye kartı kodu zorunludur.")
            .MaximumLength(50).WithMessage("Hediye kartı kodu en fazla 50 karakter olabilir.");
    }
}
