using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetFlashSaleById;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetFlashSaleByIdQueryValidator : AbstractValidator<GetFlashSaleByIdQuery>
{
    public GetFlashSaleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Flash Sale ID'si zorunludur.");
    }
}
