using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetGiftCardById;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetGiftCardByIdQueryValidator() : AbstractValidator<GetGiftCardByIdQuery>
{
    public GetGiftCardByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Hediye kartı ID'si zorunludur.");
    }
}
