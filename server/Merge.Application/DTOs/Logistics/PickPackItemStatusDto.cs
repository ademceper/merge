namespace Merge.Application.DTOs.Logistics;

public record PickPackItemStatusDto(
    bool IsPicked,
    bool IsPacked,
    string? Location = null
);
