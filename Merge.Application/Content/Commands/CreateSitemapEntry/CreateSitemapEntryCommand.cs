using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateSitemapEntry;

public record CreateSitemapEntryCommand(
    string Url,
    string PageType,
    Guid? EntityId = null,
    string ChangeFrequency = "weekly",
    decimal Priority = 0.5m
) : IRequest<SitemapEntryDto>;

