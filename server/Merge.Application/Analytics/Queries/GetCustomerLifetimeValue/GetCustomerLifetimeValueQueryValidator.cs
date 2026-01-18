using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;

public class GetCustomerLifetimeValueQueryValidator : AbstractValidator<GetCustomerLifetimeValueQuery>
{
    public GetCustomerLifetimeValueQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Müşteri ID zorunludur");
    }
}

