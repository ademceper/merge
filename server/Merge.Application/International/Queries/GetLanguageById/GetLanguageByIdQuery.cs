using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetLanguageById;

public record GetLanguageByIdQuery(Guid Id) : IRequest<LanguageDto?>;

