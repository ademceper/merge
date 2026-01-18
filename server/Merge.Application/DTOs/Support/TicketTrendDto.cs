namespace Merge.Application.DTOs.Support;


public record TicketTrendDto(
    DateTime Date,
    int Opened,
    int Resolved,
    int Closed
);
