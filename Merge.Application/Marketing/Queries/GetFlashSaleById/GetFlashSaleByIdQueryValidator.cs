using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetFlashSaleById;

public class GetFlashSaleByIdQueryValidator : AbstractValidator<GetFlashSaleByIdQuery>
{
    public GetFlashSaleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Flash Sale ID'si zorunludur.");
    }
}
