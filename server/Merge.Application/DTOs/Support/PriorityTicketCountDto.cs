using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Support;


public record PriorityTicketCountDto(
    string Priority,
    int Count,
    decimal Percentage
);
