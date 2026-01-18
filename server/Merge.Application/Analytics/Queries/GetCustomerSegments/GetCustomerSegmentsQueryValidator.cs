using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetCustomerSegments;

public class GetCustomerSegmentsQueryValidator : AbstractValidator<GetCustomerSegmentsQuery>
{
    public GetCustomerSegmentsQueryValidator()
    {
        // GetCustomerSegmentsQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

