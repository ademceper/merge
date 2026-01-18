using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Support;


public record CategoryTicketCountDto(
    string Category,
    int Count,
    decimal Percentage
);
