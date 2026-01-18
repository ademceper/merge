using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetFaq;

public record GetFaqQuery(
    Guid FaqId
) : IRequest<FaqDto?>;
