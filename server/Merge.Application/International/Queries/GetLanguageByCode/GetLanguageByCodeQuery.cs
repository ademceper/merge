using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetLanguageByCode;

public record GetLanguageByCodeQuery(string Code) : IRequest<LanguageDto?>;

